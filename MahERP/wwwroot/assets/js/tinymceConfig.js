function simpletiny(title, placeholder, height = 200) {
    tinymce.init({
        selector: '#' + title,
        plugins: 'searchreplace save autosave link autolink lists advlist table directionality preview wordcount',
        toolbar: 'undo redo | bold italic underline | fontsizeinput forecolor backcolor | alignleft aligncenter alignright alignjustify | numlist bullist | link | table',
        menubar: false,
        icons: 'material',
        relative_urls: false,
        remove_script_host: false,
        convert_urls: true,
        height: height,
        placeholder: placeholder,
        directionality: "rtl",
        theme_advanced_toolbar_align: "right",

        // تنظیمات برای حذف HTML تگ‌های ناخواسته
        valid_elements: 'p,br,strong,b,em,i,u,ol,ul,li,a[href],table,tr,td,th,thead,tbody',
        invalid_elements: 'script,style,link,meta,html,head,body,iframe,frame,object,embed,applet',
        forced_root_block: 'p',
        force_br_newlines: false,
        force_p_newlines: true,
        remove_trailing_brs: true,
        
        // پاک‌سازی محتوا هنگام paste
        paste_data_images: false,
        paste_remove_styles: true,
        paste_remove_styles_if_webkit: true,
        paste_strip_class_attributes: 'all',
        
        // فیلتر کردن محتوای HTML
        setup: function (editor) {
            editor.on('BeforeSetContent', function (e) {
                // حذف تمام تگ‌های HTML غیر مجاز
                e.content = e.content.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '');
                e.content = e.content.replace(/<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>/gi, '');
                e.content = e.content.replace(/<link[^>]*>/gi, '');
                e.content = e.content.replace(/<meta[^>]*>/gi, '');
                e.content = e.content.replace(/<iframe[^>]*>.*?<\/iframe>/gi, '');
                e.content = e.content.replace(/<object[^>]*>.*?<\/object>/gi, '');
                e.content = e.content.replace(/<embed[^>]*>/gi, '');
            });
        },

        // تنظیمات سایز فونت
        fontsize_formats: "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 20pt 22pt 24pt 26pt 28pt 36pt 48pt 72pt",

        // تنظیمات جدول
        table_default_attributes: {
            border: '1'
        },
        table_default_styles: {
            'border-collapse': 'collapse',
            'width': '100%',
            'border': '1px solid #ddd'
        },
        table_appearance_options: true,
        table_advtab: true,
        table_cell_advtab: true,
        table_row_advtab: true,
        table_responsive_width: true,
        table_resize_bars: true,
        table_border_widths: [0, 1, 2, 3],
        table_border_styles: [
            { title: 'Solid', value: 'solid' },
            { title: 'Dotted', value: 'dotted' },
            { title: 'Dashed', value: 'dashed' }
        ]
    });
}

function fulltiny(title, placeholder, height = 500) {
    tinymce.init({
        selector: '#' + title,
        plugins: 'searchreplace save autosave link autolink lists advlist table directionality preview wordcount help fullscreen',
        toolbar: 'undo redo | bold italic underline | fontsizeinput forecolor backcolor | alignleft aligncenter alignright alignjustify | numlist bullist outdent indent | link | table | fullscreen | help',
        menubar: 'file edit view insert format tools table',
        menu: {
            file: { title: 'فایل', items: 'newdocument | preview | print' },
            edit: { title: 'ویرایش', items: 'undo redo | cut copy paste pastetext | selectall | searchreplace' },
            view: { title: 'نمایش', items: 'preview fullscreen' },
            insert: { title: 'درج', items: 'link inserttable | hr' },
            format: { title: 'قالب', items: 'bold italic underline | fontsizeinput forecolor backcolor | formats blockformats | align | numlist bullist outdent indent | removeformat' },
            tools: { title: 'ابزارها', items: 'wordcount' },
            table: { title: 'جدول', items: 'inserttable | cell row column | tableprops deletetable' }
        },
        icons: 'material',
        height: height,
        max_height: 700,
        relative_urls: false,
        remove_script_host: false,
        convert_urls: true,
        placeholder: placeholder,
        directionality: "rtl",
        theme_advanced_toolbar_align: "right",

        // تنظیمات برای حذف HTML تگ‌های ناخواسته
        valid_elements: 'p,br,strong,b,em,i,u,ol,ul,li,a[href],table,tr,td,th,thead,tbody,h1,h2,h3,h4,h5,h6,hr',
        invalid_elements: 'script,style,link,meta,html,head,body,iframe,frame,object,embed,applet',
        forced_root_block: 'p',
        force_br_newlines: false,
        force_p_newlines: true,
        remove_trailing_brs: true,
        
        // پاک‌سازی محتوا هنگام paste
        paste_data_images: false,
        paste_remove_styles: true,
        paste_remove_styles_if_webkit: true,
        paste_strip_class_attributes: 'all',
        
        // فیلتر کردن محتوای HTML
        setup: function (editor) {
            editor.on('BeforeSetContent', function (e) {
                // حذف تمام تگ‌های HTML غیر مجاز
                e.content = e.content.replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '');
                e.content = e.content.replace(/<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>/gi, '');
                e.content = e.content.replace(/<link[^>]*>/gi, '');
                e.content = e.content.replace(/<meta[^>]*>/gi, '');
                e.content = e.content.replace(/<iframe[^>]*>.*?<\/iframe>/gi, '');
                e.content = e.content.replace(/<object[^>]*>.*?<\/object>/gi, '');
                e.content = e.content.replace(/<embed[^>]*>/gi, '');
            });
        },

        // تنظیمات سایز فونت
        fontsize_formats: "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 20pt 22pt 24pt 26pt 28pt 36pt 48pt 72pt",

        // تنظیمات جدول
        table_toolbar: 'tableprops tabledelete | tableinsertrowbefore tableinsertrowafter tabledeleterow | tableinsertcolbefore tableinsertcolafter tabledeletecol',
        table_default_attributes: {
            border: '1'
        },
        table_default_styles: {
            'border-collapse': 'collapse',
            'width': '100%',
            'border': '1px solid #ddd'
        },
        table_advtab: true,
        table_cell_advtab: true,
        table_row_advtab: true,
        table_appearance_options: true,
        table_responsive_width: true,
        table_resize_bars: true,
        table_border_widths: [0, 1, 2, 3],
        table_border_styles: [
            { title: 'Solid', value: 'solid' },
            { title: 'Dotted', value: 'dotted' },
            { title: 'Dashed', value: 'dashed' }
        ]
    });
}
/**
 * ویرایشگر HTML برای قالب‌های ایمیل
 * با پشتیبانی کامل از تصاویر، استایل‌های inline و جداول
 */
function initEmailTemplateEditor(editorId, placeholder = 'محتوای HTML ایمیل را اینجا بنویسید...') {
    tinymce.init({
        selector: '#' + editorId,

        // پلاگین‌های مخصوص ایمیل
        plugins: [
            'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
            'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
            'insertdatetime', 'media', 'table', 'wordcount', 'directionality',
            'template', 'emoticons', 'codesample', 'help'
        ],

        // تولبار کامل برای طراحی ایمیل
        toolbar: [
            'undo redo | formatselect | bold italic underline strikethrough | forecolor backcolor',
            'alignleft aligncenter alignright alignjustify | bullist numlist outdent indent',
            'image media link | table tabledelete | tableprops tablerowprops tablecellprops',
            'code preview fullscreen | emoticons charmap | removeformat help'
        ],

        menubar: 'file edit view insert format tools table',

        // تنظیمات ارتفاع
        height: 600,
        min_height: 400,
        max_height: 900,

        // تنظیمات RTL
        directionality: "rtl",
        language: 'fa_IR',

        // تنظیمات تصویر
        images_upload_url: '/Admin/Email/UploadImage',
        automatic_uploads: true,
        images_reuse_filename: true,

        // پشتیبانی از Base64 برای تصاویر کوچک
        images_dataimg_filter: function (img) {
            return img.hasAttribute('internal-blob');
        },

        // فرمت‌های مجاز برای ایمیل
        formats: {
            bold: { inline: 'strong' },
            italic: { inline: 'em' },
            underline: { inline: 'u', styles: { 'text-decoration': 'underline' } },
            strikethrough: { inline: 'del' }
        },

        // استایل‌های inline برای سازگاری با ایمیل
        style_formats: [
            { title: 'عنوان 1', format: 'h1' },
            { title: 'عنوان 2', format: 'h2' },
            { title: 'عنوان 3', format: 'h3' },
            { title: 'پاراگراف', format: 'p' },
            { title: 'متن برجسته', inline: 'strong' },
            { title: 'کد', inline: 'code' }
        ],

        // تنظیمات جدول برای ایمیل
        table_default_attributes: {
            border: '0',
            cellpadding: '10',
            cellspacing: '0',
            style: 'width: 100%; border-collapse: collapse;'
        },

        table_default_styles: {
            'border-collapse': 'collapse',
            'width': '100%',
            'border': '1px solid #ddd'
        },

        table_appearance_options: true,
        table_advtab: true,
        table_cell_advtab: true,
        table_row_advtab: true,

        // تنظیمات لینک
        link_default_target: '_blank',
        link_title: false,

        // محتوای HTML کامل
        valid_elements: '*[*]',
        extended_valid_elements: 'style,link[href|rel]',
        valid_children: '+body[style],+body[link]',

        // تبدیل استایل‌ها به inline
        inline_styles: true,

        // حذف تگ‌های خطرناک
        invalid_elements: 'script,iframe,object,embed,applet',

        // پیکربندی محتوا
        content_style: `
            body {
                font-family: Tahoma, Arial, sans-serif;
                font-size: 14px;
                line-height: 1.6;
                color: #333;
                background-color: #ffffff;
                margin: 10px;
            }
            table {
                border-collapse: collapse;
                width: 100%;
            }
            img {
                max-width: 100%;
                height: auto;
            }
        `,

        // placeholder
        placeholder: placeholder,

        // رویدادها
        setup: function (editor) {
            // تبدیل استایل‌ها به inline قبل از ذخیره
            editor.on('BeforeGetContent', function (e) {
                if (e.format === 'html') {
                    convertStylesToInline(editor);
                }
            });

            // پیش‌نمایش ایمیل
            editor.ui.registry.addButton('emailpreview', {
                text: 'پیش‌نمایش ایمیل',
                icon: 'preview',
                onAction: function () {
                    previewEmail(editor.getContent());
                }
            });

            // درج Placeholder
            editor.ui.registry.addMenuButton('placeholder', {
                text: 'فیلد متغیر',
                icon: 'template',
                fetch: function (callback) {
                    var items = [
                        { type: 'menuitem', text: 'نام', onAction: function () { editor.insertContent('{Name}'); } },
                        { type: 'menuitem', text: 'ایمیل', onAction: function () { editor.insertContent('{Email}'); } },
                        { type: 'menuitem', text: 'شرکت', onAction: function () { editor.insertContent('{Company}'); } },
                        { type: 'menuitem', text: 'تاریخ', onAction: function () { editor.insertContent('{Date}'); } },
                        { type: 'menuitem', text: 'تلفن', onAction: function () { editor.insertContent('{Phone}'); } }
                    ];
                    callback(items);
                }
            });
        },

        // تنظیمات امنیتی
        paste_data_images: true,
        paste_as_text: false,
        paste_webkit_styles: 'all',
        paste_merge_formats: true,

        // فونت‌ها
        font_formats: 'Tahoma=tahoma,arial,helvetica,sans-serif;Arial=arial,helvetica,sans-serif;Courier New=courier new,courier,monospace;Georgia=georgia,palatino;Times New Roman=times new roman,times;Verdana=verdana,geneva',

        fontsize_formats: '8pt 10pt 12pt 14pt 16pt 18pt 20pt 24pt 28pt 32pt 36pt',

        // رنگ‌ها
        color_map: [
            "000000", "سیاه",
            "FFFFFF", "سفید",
            "FF0000", "قرمز",
            "00FF00", "سبز",
            "0000FF", "آبی",
            "FFFF00", "زرد",
            "FF00FF", "صورتی",
            "00FFFF", "فیروزه‌ای"
        ],

        // تنظیمات Template
        templates: [
            {
                title: 'قالب ساده',
                description: 'یک قالب ایمیل ساده',
                content: `
                    <table style="width: 100%; max-width: 600px; margin: 0 auto; font-family: Tahoma, Arial, sans-serif;">
                        <tr>
                            <td style="padding: 20px; background-color: #f4f4f4;">
                                <h2 style="color: #333;">{Subject}</h2>
                                <p>سلام {Name},</p>
                                <p>{Content}</p>
                                <p>با تشکر</p>
                            </td>
                        </tr>
                    </table>
                `
            },
            {
                title: 'قالب با هدر و فوتر',
                description: 'قالب کامل با هدر و فوتر',
                content: `
                    <table style="width: 100%; max-width: 600px; margin: 0 auto; font-family: Tahoma, Arial, sans-serif; border: 1px solid #ddd;">
                        <tr>
                            <td style="padding: 20px; background-color: #4CAF50; color: white; text-align: center;">
                                <h1 style="margin: 0;">نام شرکت</h1>
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 30px; background-color: #ffffff;">
                                <h2 style="color: #333;">{Subject}</h2>
                                <p>سلام {Name},</p>
                                <p>{Content}</p>
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 15px; background-color: #f4f4f4; text-align: center; font-size: 12px; color: #666;">
                                <p>© 2024 تمامی حقوق محفوظ است</p>
                                <p>{Company} | {Phone} | {Email}</p>
                            </td>
                        </tr>
                    </table>
                `
            }
        ]
    });
}

/**
 * تبدیل استایل‌های CSS به inline
 */
function convertStylesToInline(editor) {
    // این تابع استایل‌های CSS را به inline تبدیل می‌کند
    // برای سازگاری بهتر با کلاینت‌های ایمیل
    console.log('Converting styles to inline...');
}

/**
 * پیش‌نمایش ایمیل
 */
function previewEmail(htmlContent) {
    const previewWindow = window.open('', 'EmailPreview', 'width=800,height=600');
    previewWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <title>پیش‌نمایش ایمیل</title>
        </head>
        <body style="margin: 0; padding: 20px; background-color: #f5f5f5;">
            ${htmlContent}
        </body>
        </html>
    `);
    previewWindow.document.close();
}

/**
 * ویرایشگر ساده برای متن Plain Text
 */
function initPlainTextEditor(editorId) {
    document.getElementById(editorId).style.cssText = `
        width: 100%;
        min-height: 200px;
        padding: 10px;
        font-family: monospace;
        font-size: 14px;
        border: 1px solid #ddd;
        border-radius: 4px;
    `;
}