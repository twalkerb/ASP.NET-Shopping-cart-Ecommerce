﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Smartstore.Core.Security;
using Smartstore.Core.Stores;
using Smartstore.Core.Theming;
using Smartstore.Net;
using Smartstore.Web.Rendering;
using Smartstore.Web.Theming;

namespace Smartstore.Web.Components
{
    /// <summary>
    /// Prepares data for the preview mode (flyout) tool.
    /// </summary>
    public class PreviewToolViewComponent : SmartViewComponent
    {
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeContext _themeContext;

        public PreviewToolViewComponent(IThemeRegistry themeRegistry, IThemeContext themeContext)
        {
            _themeRegistry = themeRegistry;
            _themeContext = themeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!Services.Permissions.Authorize(Permissions.Configuration.Theme.Read))
            {
                return Empty();
            }

            var currentTheme = _themeContext.CurrentTheme;
            var currentStore = Services.StoreContext.CurrentStore;
            var themeSettings = await Services.SettingFactory.LoadSettingsAsync<ThemeSettings>(currentStore.Id);
            var cookie = Request.Cookies[CookieNames.PreviewToolOpen];

            ViewBag.Themes = (from m in _themeRegistry.GetThemeManifests(false)
                              select new SelectListItem
                              {
                                  Value = m.ThemeName,
                                  Text = m.ThemeTitle,
                                  Selected = m == currentTheme
                              }).ToList();
            ViewBag.Stores = Services.StoreContext.GetAllStores().ToSelectListItems(currentStore.Id);
            ViewBag.DisableApply = themeSettings.DefaultTheme.EqualsNoCase(currentTheme.ThemeName);
            ViewBag.ToolOpen = cookie.ToBool();

            return View();
        }
    }
}
