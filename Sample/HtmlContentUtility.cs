﻿using Greatbone.Core;

namespace Greatbone.Sample
{
    public static class HtmlContentUtility
    {
        public static HtmlContent TOPBAR_(this HtmlContent h, string title)
        {
            h.T("<header data-sticky-container>");
            h.T("<div class=\"sticky\" style=\"width: 100%\" data-sticky  data-options=\"anchor: page; marginTop: 0; stickyOn: small;\">");
            h.T("<form>");
            h.T("<div class=\"top-bar\">");
            if (title != null)
            {
                h.T("<div class=\"top-bar-title\">").T(title).T("</div>");
            }
            return h;
        }

        public static HtmlContent LEFT_(this HtmlContent h)
        {
            h.T("<div class=\"top-bar-left\">");
            return h;
        }

        public static HtmlContent _LEFT(this HtmlContent h)
        {
            h.T("</div>");
            return h;
        }

        public static HtmlContent _TOPBAR(this HtmlContent h)
        {
            h.T("<div class=\"top-bar-right\">");
            h.T("<a class=\"float-right\" href=\"/my//pre/\"><i class=\"fi-shopping-cart\" style=\"font-size: 1.5rem; \"></i></a>");
            h.T("</div>");

            h.T("</div>");
            h.T("</form>");
            h.T("</div>");
            h.T("</header>");
            return h;
        }
    }
}