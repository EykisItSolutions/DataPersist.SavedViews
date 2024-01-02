

// ** JavaScript Namespace pattern

var DataPersist = {
    namespace: function (name) {
        var parts = name.split(".");
        var ns = this;

        for (var i = 0, len = parts.length; i < len; i++) {
            ns[parts[i]] = ns[parts[i]] || {};
            ns = ns[parts[i]];
        }

        return ns;
    }
};

// Namespace template -- use this as a template on local pages

DataPersist.namespace("Local").Page = (function () {

    var start = function () {
    };

    return { start: start };

})();

// Displays failure and success alert boxes

DataPersist.namespace("Utils").Alert = (function () {

    var start = function () {
       
        $("#alert-success").delay(150).animate({ top: '0px' }).delay(3400).animate({top: '-50px'}, function () {
            $(this).remove();
        });

        // Message displays longer
        $("#alert-failure").delay(150).animate({ top: '0px' }).delay(6000).animate({ top: '-50px' }, function () {
            $(this).remove();
        });
    };

    return { start: start };

})();

// Confirm delete requests

DataPersist.namespace("Utils").Confirm = (function () {

    var start = function () {


        $('.js-confirm').on('click', function (e) {

            if ($(this).attr('href') != "javascript:void(0)") {
                
                var id = $(this).data('id');
                var word = $(this).data('word');
                var returnUrl = $(this).data('return-url');
                var item = $(this).data('item');

                // opens and populates modal delete box

                if (item) $('#confirm-item').text(item);  // optional
                $('span[name="confirm-word"]').text(word);
                $('#confirm-id').val(id);
                $('#confirm-return-url').val(returnUrl);
                $('#confirm-form').attr('action', $(this).attr("href"));
                $('#confirm-modal').modal('show');
            }

            e.preventDefault();
            return false;
        });


        $('.js-submit-confirm').on('click', function (e) {

            var url = $('#confirm-form').attr('action');
            var id = $('#confirm-id').val();
            var token = $('[name="__RequestVerificationToken"]').val();
            var data = { 'id': id, '__RequestVerificationToken': token };

            $.ajax({
                url: url,
                type: 'POST',
                data: data,
                error: function (e) {
                    alert('Sorry, an error occured');
                    location = location;
                },
                success: function (data) {

                    // redirect page or refresh same page

                    var returnUrl = $('#confirm-return-url').val();

                    if (returnUrl) {
                        location = returnUrl;
                    } else {
                        location = location;
                    }
                }
            });
        });

        // set the proper referer url before editing

        $('.js-edit').on('click', function (e) {

            var url = window.location.href.split('?')[0];
            history.pushState({}, '', url + "?tab=details");

            return true;
        });
    };

    return { start: start };

})();

// Page tabs, filters, and sorters 

DataPersist.namespace("Utils").Misc = (function () {

    var start = function () {

        // related tab in detail page is clicked -> display different tab area

        $('.tabs a').on('click', function (e) {
            
            var tab = $(this).attr("href").substr(1);
            var url = window.location.href.split('?')[0];
            history.pushState({}, '', url + "?tab=" + tab);

            $(this).tab('show');
            e.preventDefault();
            return false;
        });

        // list page: standard filter dropdown changed -> submit

        $('#Filter').on('change', function () {
            $('#Page').val(1);
            $(this).closest('form').submit();
        });

        // advanced filter button is clicked

        $('.js-filter').on('click', function () {
            $('#Page').val(1);
        });

        // sort header is clicked -> submit

        $('[data-sort]').on('click', function () {
            var sort = $(this).data('sort');
            $("#Sort").val(sort);
            $("#Page").val(1);

            $(this).closest('form').submit();
        });

        // page button is clicked -> submit

        $('[data-page]').on('click', function () {
            var page = $(this).data('page');
            $("#Page").val(page);

            $(this).closest('form').submit();
        });

        // Filter toggles are clicked -> animate to different filter area

        $('.standard-toggle').on('click', function () {
            $('#standard-filter').slideDown();
            $('#advanced-filter').slideUp();
            $('#AdvancedFilter').val('False');

            $('.advanced-toggle').removeClass('active');
            $('.standard-toggle').addClass('active');
        });

        $('.advanced-toggle').on('click', function () {
            $('#standard-filter').slideUp();
            $('#advanced-filter').slideDown();
            $('#AdvancedFilter').val('True');

            $('.standard-toggle').removeClass('active');
            $('.advanced-toggle').addClass('active');
        });

        // Initialize popovers and tooltips

        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (e) {
            new bootstrap.Tooltip(e, { html: true });
        });
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(function (e) {
            new bootstrap.Popover(e, { html: true });
        });

        // Search dropdown helper

        $(".js-dropdown-item").click(function () {

            var type = $(this).data('type');
            var text = $(this).text();
           
            $(".btn-search:first-child").val(text);
            $('.btn-search:first-child').html(text + '&nbsp; <span class="caret"></span>'); //.substr(0, 3)
            $('#search-type').val(type);
        });


        // Date picker

        $('.js-date-picker').datepicker({ format: 'm/d/yyyy', autoclose: true });
    };

    // get parameter value from query string

    var getUrlParameter = function (name) {

        var url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");

        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)");
        var results = regex.exec(url);

        if (!results) return null;
        if (!results[2]) return '';

        return decodeURIComponent(results[2].replace(/\+/g, " "));
    };

    return { start: start, getUrlParameter: getUrlParameter };

})();


$(function () {
    
    DataPersist.Utils.Alert.start();
    DataPersist.Utils.Misc.start();
    DataPersist.Utils.Confirm.start();
});

