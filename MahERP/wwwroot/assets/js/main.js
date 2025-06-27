function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        console.log('Debouncing with args:', args);
        clearTimeout(timeout);
        timeout = setTimeout(() => {
            func(...args);
        }, wait);
    };
}
$(document).on('click', '[data-toggle="modal-old"]', function (event) {
    event.preventDefault();

    var trigger = $(this);
    var targetModalId = trigger.data('bs-target');

    var modalHtml = `<div aria-labelledby="modal-action" class="modal fade" data-bs-focus="false" role="dialog" id="modal-action" tabindex="-1" aria-hidden="true"></div>`;
    $('body').append(modalHtml);

    var modalElement = $(targetModalId);
    var url = trigger.attr("formaction");

    $.get(url).done(function (data) {
        modalElement.html(data);
        // استفاده از Bootstrap 5 Modal API
        var bootstrapModal = new bootstrap.Modal(modalElement[0]);
        bootstrapModal.show();
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error('Error loading modal content:', errorThrown);
    });
});


$(document).on('click', '[data-toggle="modal-ajax"]', function (event) {
    event.preventDefault();

    var trigger = $(this);
    var url = trigger.attr("href");
    if (url == undefined) {
        url = trigger.attr("formaction");
    }
    createAndShowModal(url);
});


function createAndShowModal(url) {
    var uniqueModalId = 'modal-' + Date.now();
    var modalHtml = `<div aria-labelledby="${uniqueModalId}" class="modal fade" data-bs-focus="false" role="dialog" id="${uniqueModalId}" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-body text-center">
                    <i class="fa fa-3x fa-spinner fa-spin text-primary my-3"></i>
                    <h3 class="mt-2 mb-3">در حال بارگذاری...</h3>
                </div>
            </div>
        </div>
    </div>`;

    $('body').append(modalHtml);
    var modalElement = document.getElementById(uniqueModalId);
    var bootstrapModal = new bootstrap.Modal(modalElement);

    // Load content into the modal via AJAX
    $.get(url)
        .done(function (data) {
            $(modalElement).find('.modal-content').html(data);

            // تنظیم select2 اگر وجود داشته باشد
            $(modalElement).find('.js-select2').attr('data-container', '#' + uniqueModalId);

            bootstrapModal.show();
        })
        .fail(function (xhr, status, error) {
            console.error('Error loading modal content:', error);
            $(modalElement).find('.modal-content').html(`
                <div class="modal-body text-center">
                    <i class="fa fa-3x fa-exclamation-triangle text-danger my-3"></i>
                    <h3 class="mt-2 mb-3">خطا در بارگذاری</h3>
                    <p>لطفا دوباره تلاش کنید.</p>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">بستن</button>
                </div>
            `);
            bootstrapModal.show();
        });

    // Handle modal hidden event to remove it from the DOM
    modalElement.addEventListener('hidden.bs.modal', function () {
        modalElement.remove();
    });
}


function ModalTrigger(input) {
    event.preventDefault();
    var url = $(input).attr('href');
    createAndShowModal(url);
}


$(document).on('hidden.bs.modal', '.modal', function () {
    $(this).remove();
});


$(document).on('click', ' [data-save="modal-ajax-save"]', function (event) {
    event.preventDefault();
    var $button = $(this); // Cache the button element
    $button.prop('disabled', true); // Disable the button immediately
    var $modal = $button.closest('.modal');


    if (typeof tinymce != 'undefined') {

        tinymce.triggerSave();

    }
    var form = $button.closest('.modal').find('form');
    var formData = new FormData(form[0]);
    $('input[type="checkbox"]').each(function () {
        var checkbox = $(this);
        var isChecked = checkbox.prop('checked');
        var checkboxName = checkbox.attr('id');
        // اضافه کردن وضعیت چک باکس به داده‌های ارسالی
        formData.append(checkboxName, isChecked);
    });


    $.ajax({
        url: form.attr("action"),
        type: "POST",
        data: formData,
        //contentType: false,
        contentType: false,  // تعیین نوع محتوا به false برای استفاده از FormData
        processData: false,
        success: function (response) {
            if (response.status == "redirect") {
                $(location).prop('href', response.redirectUrl)
            }
            else if (response.status == "update-view") {
                if (response.viewList && response.viewList.length) {
                    $.each(response.viewList, function (index, item) {
                        // Code to be executed for each element
                        $("#" + item.elementId).html(item.view.result);
                    });
                }

                // استفاده از Bootstrap 5 API برای بستن مودال
                var modalElement = $button.closest('.modal')[0];
                var bootstrapModal = bootstrap.Modal.getInstance(modalElement);
                if (bootstrapModal) {
                    bootstrapModal.hide();
                }
                setTimeout(() => {
                    $modal.remove();
                }, 300);

            }

            else if (response.status == "temp-save") {
                if (response.viewList && response.viewList.length) {

                    $.each(response.viewList, function (index, item) {
                        // Code to be executed for each element
                        $("#" + item.elementId).html(item.view.result);
                        SendResposeMessageInView(response);

                    });
                }
            }
            if (response.message != undefined) {
                SendResposeMessage(response.message);

            }

        },
        error: function (xhr) {
            $button.prop('disabled', false); // Disable the button immediately

            // Error: Display the error message in the modal
            var errors = xhr.responseJSON; // The ModelState object returned as JSON
            for (var key in errors) {
                Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
            }

        }
    });
    $button.prop('disabled', false); // Enable the button 

});


$(document).on('click', ' [data-save="modal"]', function (event) {
    var $button = $(this); // Cache the button element
    $button.prop('disabled', true); // Disable the button immediately
    var form = $button.closest('.modal').find('form');

    $(form).submit();
    //var actionUrl = form.attr('action');
    //var sendData = form.serialize();
    //$.post(actionUrl, sendData).done(function (data) {
    //})


})


//Modalelement.on('click', '[data-bs-dismiss="modal"]', function (event) {

//    $(".modal-contents").empty();


//})
// تابع اصلی برای لود partial view
function loadPartialView($target, isInput = false) {
    var viewId = $target.data('target');
    Dashmix.block('state_loading', viewId);

    var targetUr = $target.data('url');
    var href = $target.attr('href');
    var formaction = $target.attr('formaction');
    var formUrl = $target.closest('form').attr('action');

    var url = targetUr != undefined ? targetUr : href != undefined ? href : formaction != undefined ? formaction : formUrl;

    // اگر از input باشد، داده‌های فرم را جمع‌آوری و با POST ارسال کنیم
    if (isInput) {
        var formData = new FormData();

        // اضافه کردن مقادیر select‌ها اگر data-sclass وجود داشته باشد
        var selects = $target.data('sclass');
        if (typeof selects !== 'undefined') {
            const selectElements = document.querySelectorAll('.' + selects);
            selectElements.forEach(select => {
                var name = select.name;
                var value = select.value;
                formData.append(name, value);
            });
        }

        // اضافه کردن مقدار input
        var name = $target.attr('name');
        var value = $target.val();
        formData.append(name, value);

        // تنظیم page به 1 برای فیلترهای جدید
        formData.append('page', '1');

        // ارسال درخواست با POST
        $.ajax({
            url: url,
            type: 'post',
            data: formData,
            processData: false,
            contentType: false
        }).done(function (response) {
            $(viewId).html(response);
            Dashmix.block('state_normal', viewId);
        }).fail(function (xhr) {
            Dashmix.block('state_normal', viewId);
            var errors = xhr.responseJSON;
            if (errors) {
                for (var key in errors) {
                    Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
                }
            } else {
                Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: 'خطایی در بارگذاری محتوا رخ داد.' });
            }
        });
    } else {
        // برای کلیک روی لینک‌ها یا دکمه‌ها، از GET استفاده کنیم
        $.get(url).done(function (response) {
            $(viewId).html(response);

            var functionName = $target.data('callback');
            if (functionName && typeof window[functionName] === 'function') {
                window[functionName]();
            }
            Dashmix.block('state_normal', viewId);
        }).fail(function (xhr) {
            Dashmix.block('state_normal', viewId);
            var errors = xhr.responseJSON;
            if (errors) {
                for (var key in errors) {
                    Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
                }
            } else {
                Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: 'خطایی در بارگذاری محتوا رخ داد.' });
            }
        });
    }
}
// شنونده برای کلیک روی دکمه‌های partialview
$(document).on('click', '[data-toggle="partialview"]', function (event) {
    event.preventDefault();
    loadPartialView($(this));
});
$(document).on('keyup', 'input[type="text"][data-toggle="partialview"]', function (event) {
    var $target = $(this);
    debounce(function () {
        loadPartialView($target, true);
    }, 300)();
});

$(document).on('change', 'select[data-toggle="partialview"]', function (event) {

    var $target = $(this);
    var viewId = $target.data('target');
    Dashmix.block('state_loading', viewId);


    var data = $target.find(":selected").val();
    var url = $target.data('url');
    $.get(url, { selectedId: data }).done(function (data) {
        $("#" + viewId).html(data);
        Dashmix.block('state_normal', viewId);

    })
});


$(document).on('change', 'select[data-toggle="partialviewmultiple"]', function (event) {

    var $target = $(this);
    var viewId = $target.data('target');
    Dashmix.block('state_loading', viewId.replace(/^#/, ''));

    var targetUrl = $target.data('url');

    var data = $target.find(":selected").val();

    var url = targetUrl != undefined ? targetUrl : href != undefined ? href : formaction != undefined ? formaction : formUrl;

    $.get(url, { selected: data }).done(function (data) {
        $.each(data.viewList, function (index, item) {
            // Code to be executed for each element
            $("#" + item.elementId).html(item.view.result);

        });
        SendResposeMessage(response);
        Dashmix.block('state_normal', viewId.replace(/^#/, ''));

    })


});


$(document).on('change', 'input[type="checkbox"][data-toggle="partialview"]', function (event) {
    $(viewId).addClass("block  block-mode-loading-refresh block-mode-loading");

    var $target = $(this);
    var viewId = $target.data('target');
    Dashmix.block('state_loading', viewId.replace(/^#/, ''));

    var data = $target.prop('checked');
    var url = $target.data('url');
    $.get(url, { boolval: data }).done(function (data) {
        $("#" + viewId).html(data);

        Dashmix.block('state_normal', viewId.replace(/^#/, ''));

    })
});


$(document).on('click', 'button[data-toggle="submitformmultiple"]', function (event) {
    event.preventDefault();


    var $target = $(this);
    var viewClass = $target.data('targetc');
    var url = $target.data('url');
    $("." + viewClass).addClass("block-mode-loading");

    var form = $target.closest('form');
    var dataform = new FormData(form[0]); // Use FormData directly

    if (typeof tinymce != 'undefined') {

        tinymce.triggerSave();

    }
    $('input[type="checkbox"]').each(function () {
        var checkbox = $(this);
        var isChecked = checkbox.prop('checked');
        var checkboxName = checkbox.attr('id');
        // اضافه کردن وضعیت چک باکس به داده‌های ارسالی
        dataform.append(checkboxName, isChecked);
    });

    // Select all <select> elements inside the form
    form.find('select').each(function () {
        var selectName = $(this).attr('name'); // Get the name attribute of the select
        var selectValue = $(this).val(); // Get the value of the select

        // Append the select name and value to the FormData object
        dataform.append(selectName, selectValue);
    });

    $("." + viewClass).addClass("block-mode-loading");


    var targetUr = $target.data('url');
    var href = $(this).attr('href');
    var formaction = $(this).attr("formaction");

    var formUrl = $(this).closest('form').attr("action");

    var url = targetUr != undefined ? targetUr : href != undefined ? href : formaction != undefined ? formaction : formUrl;
    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false

    }).done(function (response) {


        $.each(response.viewList, function (index, item) {
            // Code to be executed for each element
            $("#" + item.elementId).html(item.view.result);

        });
        convertprices();
        $("." + viewClass).removeClass("block-mode-loading");
        SendResposeMessage(response);

    }).fail(function (xhr) {
        $("." + viewClass).removeClass("block-mode-loading");


        // Error: Display the error message in the modal
        var errors = xhr.responseJSON; // The ModelState object returned as JSON
        for (var key in errors) {
            Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
        }

    });


})

// تابع Debounce برای جلوگیری از ارسال درخواست‌های مکرر
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// تابع اصلی برای ارسال فرم
function submitForm($target) {
    var viewId = $target.data('target');

    var form = $target.closest('form');


    Dashmix.block('state_loading', viewId);

    if (typeof tinymce != 'undefined') {
        tinymce.triggerSave();
    }

    var dataform = new FormData(form[0]); // Use FormData directly

    // اضافه کردن وضعیت چک‌باکس‌ها
    $('input[type="checkbox"]').each(function () {
        var checkbox = $(this);
        var isChecked = checkbox.prop('checked');
        var checkboxName = checkbox.attr('id');
        if (checkboxName) { // اطمینان از وجود id
            dataform.append(checkboxName, isChecked);
        }
    });

    // تنظیم page به 1 برای فیلترهای جدید

    var targetUr = $target.data('url');
    var href = $target.attr('href');
    var formaction = $target.attr('formaction');
    var formUrl = form.attr('action');

    var url = targetUr != undefined ? targetUr : href != undefined ? href : formaction != undefined ? formaction : formUrl;

    if (!url) {
        console.error('URL is undefined');
        Dashmix.block('state_normal', viewId);
        return;
    }

    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false
    }).done(function (data) {
        $(viewId).html(data);
        convertprices();
        Dashmix.block('state_normal', viewId);
    }).fail(function (xhr) {
        Dashmix.block('state_normal', viewId);
        var errors = xhr.responseJSON;
        if (errors) {
            for (var key in errors) {
                Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
            }
        } else {
            Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: 'خطایی در ارسال درخواست رخ داد.' });
        }
    });
}

// شنونده برای کلیک روی دکمه‌های submitform
$(document).on('click', '[data-toggle="submitform"]', function (event) {
    event.preventDefault();
    submitForm($(this));
});

// شنونده برای keyup روی input‌های با data-toggle="submitform"
$(document).on('keyup', 'input[type="text"][data-toggle="submitform"]', function (event) {
    var $target = $(this); // صراحتاً $target را تعریف می‌کنیم
    debounce(function () {
        submitForm($target);
    }, 500)();
});
$(document).on('click', '[data-toggle="swal-asp"]', function (event) {
    event.preventDefault();
    var $target = $(this);
    var viewId = $target.data('target');
    var title = $target.data('title');
    var description = $target.data('desc');
    var href = $target.attr('href');

    Dashmix.block('state_loading', viewId.replace(/^#/, ''));


    var swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: "btn btn-success",
            cancelButton: "btn btn-danger"
        },
        buttonsStyling: false
    });
    swalWithBootstrapButtons.fire({
        title: title,
        text: description,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "تایید",
        cancelButtonText: "لغو",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {

            $(location).prop('href', href)


        }
    });

});


$(document).on('click', '[data-toggle="swal-ajax"]', function (event) {
    event.preventDefault();
    var $target = $(this);
    var viewId = $target.data('target');
    var title = $target.data('title');
    var description = $target.data('desc');



    var swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: "btn btn-success",
            cancelButton: "btn btn-danger"
        },
        buttonsStyling: false
    });
    swalWithBootstrapButtons.fire({
        title: title,
        text: description,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "تایید",
        cancelButtonText: "لغو",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {



            if (typeof tinymce != 'undefined') {

                tinymce.triggerSave();

            }
            var form = $(this).closest('form');
            var formData = new FormData(form[0]);
            $('input[type="checkbox"]').each(function () {
                var checkbox = $(this);
                var isChecked = checkbox.prop('checked');
                var checkboxName = checkbox.attr('id');
                // اضافه کردن وضعیت چک باکس به داده‌های ارسالی
                formData.append(checkboxName, isChecked);
            });

            var targetUr = $target.data('url');
            var href = $(this).attr('href');
            var formaction = $(this).attr("formaction");

            var formUrl = $(this).closest('form').attr("action");

            var url = targetUr != undefined ? targetUr : href != undefined ? href : formaction != undefined ? formaction : formUrl;
            $.ajax({
                url: url,
                type: "POST",
                data: formData,
                //contentType: false,
                contentType: false,  // تعیین نوع محتوا به false برای استفاده از FormData
                processData: false,
                success: function (response) {
                    if (response.status == "redirect") {
                        $(location).prop('href', response.redirectUrl)
                    }
                    else if (response.status == "update-view") {
                        if (response.viewList && response.viewList.length) {

                            $.each(response.viewList, function (index, item) {
                                // Code to be executed for each element
                                $("#" + item.elementId).html(item.view.result);
                            });
                        }
                        // اصلاح برای Bootstrap 5
                        var modalElement = document.querySelector('.modal.show');
                        if (modalElement) {
                            var bootstrapModal = bootstrap.Modal.getInstance(modalElement);
                            if (bootstrapModal) {
                                bootstrapModal.hide();
                            }
                        }

                    }

                    else if (response.status == "temp-save") {
                        if (response.viewList && response.viewList.length) {

                            $.each(response.viewList, function (index, item) {
                                // Code to be executed for each element
                                $("#" + item.elementId).html(item.view.result);
                                SendResposeMessageInView(response);

                            });
                        }
                    }
                    else if (response.status == "download") {
                        // ایجاد لینک مخفی برای دانلود
                        var link = document.createElement('a');
                        link.href = response.downloadUrl;
                        link.download = ''; // نام فایل توسط سرور تعیین می‌شود
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    }
                    if (response.message != undefined) {
                        SendResposeMessage(response.message);
                    }
                },
                error: function (xhr) {
                    // حذف اشاره به $button که تعریف نشده بود
                    console.error('AJAX Error:', xhr);

                    // Error: Display the error message in the modal
                    var errors = xhr.responseJSON; // The ModelState object returned as JSON
                    for (var key in errors) {
                        Dashmix.helpers('jq-notify', { type: 'danger', icon: 'fa fa-times me-1', message: errors[key] });
                    }

                }
            });
        }
    });

});



function SendResposeMessageInView(response) {

    $.each(response.viewList, function (index, viewdata) {
        $.each(viewdata.messages, function (index, messagerow) {

            var icons = {
                "success": 'fa fa-check',
                "warning": 'fa fa-exclamation-triangle',
                "info": 'fa fa-info',
                "error": 'fa fa-times'
            }; var notificationTypes = {
                "success": 'success',
                "warning": 'warning',
                "info": 'info',
                "error": 'error'
            };
            // Determine notification type based on status or default to 'info'
            var notificationType = notificationTypes[messagerow.status] || 'info';

            // Determine icon based on status
            var icon = icons[messagerow.status] || 'fa fa-info';

            Dashmix.helpers('jq-notify', {
                type: notificationType,
                icon: icon + ' me-1', // Add space to separate icon classes
                message: messagerow.text
            });
        });
    });

}

function SendResposeMessage(messages) {
    $.each(messages, function (index, messagerow) {

        var icons = {
            "success": 'fa fa-check',
            "warning": 'fa fa-exclamation-triangle',
            "info": 'fa fa-info',
            "error": 'fa fa-times'
        }; var notificationTypes = {
            "success": 'success',
            "warning": 'warning',
            "info": 'info',
            "error": 'error'
        };
        // Determine notification type based on status or default to 'info'
        var notificationType = notificationTypes[messagerow.status] || 'info';

        // Determine icon based on status
        var icon = icons[messagerow.status] || 'fa fa-info';

        Dashmix.helpers('jq-notify', {
            type: notificationType,
            icon: icon + ' me-1', // Add space to separate icon classes
            message: messagerow.text
        });
    });

}


function jstreeChanged(url, data) {
    var dataform = new FormData(); // Use FormData directly
    dataform.append("jstreeData", data);

    $.ajax({
        type: "post",
        url: url,
        data: dataform,
        contentType: false,
        processData: false

    }).done(function (response) {

        $.each(response.viewList, function (index, item) {
            // Code to be executed for each element
            $("#" + item.elementId).html(item.view.result);


        });


    }).fail(function (response) {

    });

}


var convertprices = function () {
    var prices = document.getElementsByClassName("cprices");

    for (var i = 0; i < prices.length; i++) {

        prices[i].textContent = prices[i].textContent.replace(/,/g, '');
        var price = parseInt(prices[i].textContent).toLocaleString('en');
        prices[i].textContent = price;
    }
}


$(document).on('focus', '.select2-selection.select2-selection--single', function (e) {
    $(this).closest(".select2-container").siblings('select:enabled').select2('open');
});


$(document).on('select2:open', () => {
    document.querySelector('.select2-container--open .select2-search__field').focus();
});



function copyToClipboard(Data) {

    event.preventDefault();
    var $target = $(Data);
    var textToCopy = $target.data('url');


    // navigator clipboard api needs a secure context (https)
    if (navigator.clipboard && window.isSecureContext) {
        // navigator clipboard api method'
        return navigator.clipboard.writeText(textToCopy);
    } else {
        // text area method
        let textArea = document.createElement("textarea");
        textArea.value = textToCopy;
        // make the textarea out of viewport
        textArea.style.position = "fixed";
        textArea.style.left = "-999999px";
        textArea.style.top = "-999999px";
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        return new Promise((res, rej) => {
            // here the magic happens
            document.execCommand('copy') ? res() : rej();
            textArea.remove();
        });
    }
}
// فرمت کردن اعداد در فیلدهای ورودی با کلاس number-format
$(document).on('input', '.number-format', function () {
    let value = $(this).val().replace(/,/g, '').replace(/\.\d+/, ''); // حذف کاماها و اعشار
    if (value) {
        value = parseInt(value, 10).toLocaleString('en-US'); // اضافه کردن کاماها
        $(this).val(value);
    }
});

// حذف کاماها قبل از ارسال فرم
$(document).on('submit', 'form', function () {
    $('.number-format').each(function () {
        let value = $(this).val().replace(/,/g, ''); // حذف کاماها
        $(this).val(value);
    });
    return true; // ادامه ارسال فرم
});

// اعمال فرمت اولیه بعد از لود صفحه
$(document).ready(function () {
    $('.number-format').each(function () {
        let value = $(this).val();
        if (value) {
            value = value.toString().replace(/,/g, '').replace(/\.\d+/, '');
            if (value !== '') {
                value = parseInt(value, 10).toLocaleString('en-US');
                $(this).val(value);
            }
        }
    });
});

// اعمال فرمت پس از نمایش مودال (برای عناصر داینامیک)
$(document).on('shown.bs.modal', function () {
    $('.number-format').each(function () {
        let value = $(this).val();
        if (value) {
            value = value.toString().replace(/,/g, '').replace(/\.\d+/, '');
            if (value !== '') {
                value = parseInt(value, 10).toLocaleString('en-US');
                $(this).val(value);
            }
        }
    });
});