function initArabicDataTable(selector, options) {
    // Arabic language config
    var arabicLang = {
        "sEmptyTable":     "لا توجد بيانات متاحة في الجدول",
        "sLoadingRecords": "جارٍ التحميل...",
        "sProcessing":     "جارٍ المعالجة...",
        "sLengthMenu":     "أظهر _MENU_ سجلات",
        "sZeroRecords":    "لم يتم العثور على نتائج",
        "sInfo":           "إظهار _START_ إلى _END_ من أصل _TOTAL_ سجل",
        "sInfoEmpty":      "يعرض 0 إلى 0 من أصل 0 سجل",
        "sInfoFiltered":   "(منتقاة من مجموع _MAX_ سجل)",
        "sSearch":         "ابحث:",
        "oAria": {
            "sSortAscending":  ": تفعيل لترتيب العمود تصاعدياً",
            "sSortDescending": ": تفعيل لترتيب العمود تنازلياً"
        }
    };

    // Merge options if exist
    var config = Object.assign({
        paging: true,
        searching: true,
        ordering: true,
        info: true,
        lengthChange: true,
        responsive: true,
        language: arabicLang
    }, options || {});

    return $(selector).DataTable(config);
}