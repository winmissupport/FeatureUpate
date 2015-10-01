// Ajaxify
// https://github.com/browserstate/ajaxify
// Modded for Exigo use
define(["vendor/jquery.scrollto", "vendor/jquery.history"], function () {

    // Prepare our Variables
    var History = window.History,
        document = window.document;


       /* Application Specific Variables */
       contentSelector = '#site-content',
       $content = $(contentSelector).filter(':first'),
       contentNode = $content.get(0),
       $menu = $('#menu,#nav,nav:first,.nav:first').filter(':first'),
       activeClass = 'active',
       activeSelector = '.active,.selected,.current,.youarehere',
       menuChildrenSelector = '> li,> ul > li',
       completedEventName = 'statechangecomplete',

       /* Application Generic Variables */
       $window = $(window),
       $body = $(document.body),
       rootUrl = History.getRootUrl(),
       scrollOptions = {
           duration: 800,
           easing: 'swing'
       };


    // Ensure Content
       if ($content.length === 0) {
           $content = $body;
       }


    // Check to see if History.js is enabled for our Browser
    if (!History.enabled) {
        alert('Ajaxify cannot run without History.js!');
        return false;
    }


    // Ensure Content
    if ($content.length === 0) {
        $content = $body;
    }


    // Internal Helper
    $.expr[':'].internal = function (obj, index, meta, stack) {
        // Prepare
        var
            $this = $(obj),
            url = $this.attr('href') || '',
            isInternalLink;

        // Check link
        isInternalLink = url.substring(0, rootUrl.length) === rootUrl || url.indexOf(':') === -1;

        // Ignore or Keep
        return isInternalLink;
    };


    // Helpers
    var helpers = {
        documentHtml: function (html) {
            // Prepare
            var result = String(html)
                .replace(/<\!DOCTYPE[^>]*>/i, '')
                .replace(/<(html|head|body|title|meta)([\s\>])/gi, '<div class="document-$1"$2')
                .replace(/<\/(html|head|body|title|meta)\>/gi, '</div>');
            ;

            // Return
            return $.trim(result);
        }
    };


    // Define our exportable module
    var module = {
        loadUrl: function (url) {
            var State = History.getState(),
                url = url || State.url,
                relativeUrl = url.replace(rootUrl, '');

            // Set Loading
            $body.addClass('loading');

            // Start Fade Out
            // Animating to opacity to 0 still keeps the element's height intact
            // Which prevents that annoying pop bang issue when loading in new content
            //$content.animate({ opacity: 0 }, 800);

            // Ajax Request the Traditional Page
            $.ajax({
                url: url,
                success: function (data, textStatus, jqXHR) {

                    // Prepare
                    var
                        $data = $(helpers.documentHtml(data)),
                        $dataBody = $data.find('.document-body:first'),
                        $dataContent = $dataBody.find(contentSelector).filter(':first'),
                        $menuChildren, contentHtml;

                    // Fetch the content
                    contentHtml = $dataContent.html() || $data.html();
                    if (!contentHtml) {
                        document.location.href = url;
                        return false;
                    }

                    // Update the menu
                    $menuChildren = $menu.find(menuChildrenSelector);
                    $menuChildren.filter(activeSelector).removeClass(activeClass);
                    $menuChildren = $menuChildren.has('a[href^="' + relativeUrl + '"],a[href^="/' + relativeUrl + '"],a[href^="' + url + '"]');
                    if ($menuChildren.length === 1) { $menuChildren.addClass(activeClass); }

                    // Update the content
                    $content.stop(true, true);
                    $content.html(contentHtml).css('opacity', 100).show(); /* you could fade in here if you'd like */

                    // Update the title
                    document.title = $data.find('.document-title:first').text();
                    try {
                        document.getElementsByTagName('title')[0].innerHTML = document.title.replace('<', '&lt;').replace('>', '&gt;').replace(' & ', ' &amp; ');
                    }
                    catch (Exception) { }

                    // Complete the change
                    if ($body.ScrollTo || false) { $body.ScrollTo(scrollOptions); } /* http://balupton.com/projects/jquery-scrollto */
                    $body.removeClass('loading');
                    $window.trigger(completedEventName);

                    // Inform Google Analytics of the change
                    if (typeof window._gaq !== 'undefined') {
                        window._gaq.push(['_trackPageview', relativeUrl]);
                    }

                    // Inform ReInvigorate of a state change
                    if (typeof window.reinvigorate !== 'undefined' && typeof window.reinvigorate.ajax_track !== 'undefined') {
                        reinvigorate.ajax_track(url);
                        // ^ we use the full url here as that is what reinvigorate supports
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    document.location.href = url;
                    return false;
                }
            });
        }
    };


    // Create the hooks
    $window.on('statechange', function () {
        module.loadUrl();
    });
    $(document).on('click', '.ajaxify', function (event) {
        var $this = $(this),
            url   = $this.attr('href'),
            title = $this.attr('title') || null;

        // Continue as normal for cmd clicks etc
        if (event.which == 2 || event.metaKey) { return true; }

        // Ajaxify this link
        History.pushState(null, title, url);
        event.preventDefault();
        return false;
    });


    return module;
});