﻿// jQuery extensions
if (window.jQuery) {

    // Selector for broken links
    $.extend($.expr[':'], {
        broken: function (selector) {
            var href = $(selector).attr('href');
            var isBroken =
                href == null
                || href == undefined
                || href == ''
                || href == 'javascript:;'
                || href == '#';

            return isBroken;
        }
    });

    // Serialize forms and other objects to JSON
    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        if (a.length == 0) a = this.find(':input').serializeArray()
        $.each(a, function () {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };
}