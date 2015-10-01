﻿// Forms module
define(["app"], function (app) {

    var module = {
        regex: {
            inputs: {
                'email': '^[a-zA-Z0-9_/\-/\@/\.]+$',
                'phone': '^[0-9/./\-]+$',
                'taxid': '^[0-9/./\-]+$',
                'loginname': '^[a-zA-Z0-9_\-]+$',
                'password': '^[a-zA-Z0-9_\-]+$',
                'zipcode': '^[a-zA-Z0-9/\-]+$',
                'int': '^[0-9]+$',
                'number': '^[0-9/.]+$'
            }
        },
        restrictInput: function (selector, expression) {
            $(selector).each(function () {
                var $input = $(this),
                    _regex = undefined;

                if (expression) {
                    _regex = expression;
                }
                else {
                    var regexKey = $input.data('restrict-input');
                    _regex = module.regex.inputs[regexKey] || undefined;

                    if (!_regex) {
                        console.log('Unable to find a regex for "' + regexKey + '".');
                        return;
                    }
                }

                $input.on('keypress', function (event) {
                    var regex = new RegExp(_regex);
                    var keyCode = !event.charCode ? event.which : event.charCode;
                    var key = String.fromCharCode(keyCode);

                    // Firefox and Opera need some handholding here.
                    // 0=Unknown, 8=Backspace, 13=Enter
                    if (keyCode == "0" || keyCode == "8" || keyCode == "13") return true;

                    if (!regex.test(key)) {
                        event.preventDefault();
                        return false;
                    }
                });
            });
        }
    };

    return module;

});