﻿// Cookies module
define(["app"], function (app) {
    
    var module = {
        get: function (name) {
            var dc = document.cookie;
            var prefix = name + "=";
            var begin = dc.indexOf("; " + prefix);
            if (begin == -1) {
                begin = dc.indexOf(prefix);
                if (begin != 0) return null;
            } else {
                begin += 2;
            }
            var end = document.cookie.indexOf(";", begin);
            if (end == -1) {
                end = dc.length;
            }
            return unescape(dc.substring(begin + prefix.length, end));
        },
        set: function (name, value, options) {
            document.cookie = name + "=" + escape(value) +
                ((options.expires) ? "; expires=" + options.expires.toUTCString() : "") +
                ((options.path) ? "; path=" + options.path : "") +
                ((options.domain) ? "; domain=" + options.domain : "") +
                ((options.secure) ? "; secure" : "");
        },
        clear: function (name, path, domain) {
            if (this.get(name)) {
                document.cookie = name + "=" +
                    ((path) ? "; path=" + path : "") +
                    ((domain) ? "; domain=" + domain : "") +
                    "; expires=Thu, 01-Jan-70 00:00:01 GMT";
            }
        }
    };

    return module;
    
});