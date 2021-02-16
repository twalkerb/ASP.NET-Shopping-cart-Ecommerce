﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Catalog.Categories;
using Smartstore.Core.Data;
using Smartstore.Core.Rules;
using Smartstore.Core.Scheduling;
using Smartstore.Data;
using Smartstore.Data.Batching;

namespace Smartstore.Core.Catalog.Rules
{
    /// <summary>
    /// Updates the system assignments to categories for rules.
    /// </summary>
    public partial class ProductRuleEvaluatorTask : ITask
    {
        protected readonly SmartDbContext _db;
        protected readonly IRuleService _ruleService;
        protected readonly IProductRuleProvider _productRuleProvider;

        public ProductRuleEvaluatorTask(
            SmartDbContext db,
            IRuleService ruleService,
            IProductRuleProvider productRuleProvider)
        {
            _db = db;
            _ruleService = ruleService;
            _productRuleProvider = productRuleProvider;
        }

        public async Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var count = 0;
            var numDeleted = 0;
            var numAdded = 0;
            var numCategories = 0;
            var pageSize = 500;
            var pageIndex = -1;

            var categoryIds = ctx.Parameters.ContainsKey("CategoryIds")
                ? ctx.Parameters["CategoryIds"].ToIntArray()
                : null;

            // Hooks are enabled because search index needs to be updated.
            using (var scope = new DbContextScope(_db, autoDetectChanges: false, hooksEnabled: true, deferCommit: true))
            {
                // Delete existing system mappings.
                var deleteQuery = _db.ProductCategories.Where(x => x.IsSystemMapping);
                if (categoryIds != null)
                {
                    deleteQuery = deleteQuery.Where(x => categoryIds.Contains(x.CategoryId));
                }

                await deleteQuery.BatchDeleteAsync(cancelToken);

                // Insert new product category mappings.
                var categoryQuery = _db.Categories
                    .Include(x => x.RuleSets)
                    .AsNoTracking();

                if (categoryIds != null)
                {
                    categoryQuery = categoryQuery.Where(x => categoryIds.Contains(x.Id));
                }

                var categories = await categoryQuery
                    .Where(x => x.Published && x.RuleSets.Any(y => y.IsActive))
                    .ToListAsync(cancelToken);

                numCategories = categories.Count;

                foreach (var category in categories)
                {
                    var ruleSetProductIds = new HashSet<int>();

                    // TODO: (mg) (core) Complete ProductRuleEvaluatorTask (TaskExecutionContext required).
                    //ctx.SetProgress(++count, categories.Count, $"Add product mappings for category \"{category.Name.NaIfEmpty()}\".");

                    // Execute active rule sets and collect product ids.
                    foreach (var ruleSet in category.RuleSets.Where(x => x.IsActive))
                    {
                        if (cancelToken.IsCancellationRequested)
                            return;

                        var expressionGroup = await _ruleService.CreateExpressionGroupAsync(ruleSet, (IRuleVisitor)_productRuleProvider);
                        if (expressionGroup is SearchFilterExpression expression)
                        {
                            pageIndex = -1;
                            while (true)
                            {
                                // Do not touch searchResult.Hits. We only need the product identifiers.
                                var searchResult = await _productRuleProvider.SearchAsync(new[] { expression }, ++pageIndex, pageSize);
                                ruleSetProductIds.AddRange(searchResult.HitsEntityIds);

                                if (pageIndex >= (searchResult.TotalHitsCount / pageSize))
                                {
                                    break;
                                }
                            }
                        }
                    }

                    // Add mappings.
                    if (ruleSetProductIds.Any())
                    {
                        foreach (var chunk in ruleSetProductIds.Slice(500))
                        {
                            if (cancelToken.IsCancellationRequested)
                                return;

                            foreach (var productId in chunk)
                            {
                                _db.ProductCategories.Add(new ProductCategory
                                {
                                    ProductId = productId,
                                    CategoryId = category.Id,
                                    IsSystemMapping = true
                                });

                                ++numAdded;
                            }

                            await scope.CommitAsync(cancelToken);
                        }

                        try
                        {
                            scope.DbContext.DetachEntities<ProductCategory>();
                        }
                        catch { }
                    }
                }
            }

            Debug.WriteLineIf(numDeleted > 0 || numAdded > 0, $"Deleted {numDeleted} and added {numAdded} product mappings for {numCategories} categories.");
        }
    }
}
