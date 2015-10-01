$.validator.setDefaults({

    // Elements & Classes
    errorElement: "input",

    // Settings
    ignore: ":hidden",
    focusInvalid: true,
    onsubmit: true,
    ignoreTitle: false,

    // Events
    highlight: function (element, errorClass, validClass) {
        if (element.type === 'radio') {
            this.findByName(element.name).addClass(errorClass).removeClass(validClass);
        } else {
            $(element).parents('.form-group').addClass('has-error');
        }
    },
    unhighlight: function (element, errorClass, validClass) {
        if (element.type === 'radio') {
            this.findByName(element.name).removeClass(errorClass).addClass(validClass);
        } else {
            $(element).parents('.form-group').removeClass('has-error');
        }
    }
});