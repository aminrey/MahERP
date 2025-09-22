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