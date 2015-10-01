require(["forms", "extensions"], function (forms) {
    $(function () {

        // Auto-create popovers for all links configured to be popovers
        $(document).popover({
            selector: 'a[data-toggle="popover"]',
            html: true,
            placement: 'top'
        });

        // Bind accordions to manage an active class
        $(document).on({
            'show.bs.collapse': function (event) {
                $(event.target).parents('.panel').addClass('active');
            },
            'hide.bs.collapse': function (event) {
                $(event.target).parents('.panel').removeClass('active');
            }
        });

        // Antiscroll
        $('.antiscroll').antiscroll();
    });


    // Restrict appropriate field inputs by key
    forms.restrictInput('[data-restrict-input]');
});