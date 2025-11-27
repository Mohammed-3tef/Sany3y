function initArabicDataTable(selector, options) {
    // Arabic language config
    var arabicLang = {
        emptyTable: "لا توجد بيانات متاحة في الجدول",
        loadingRecords: "جارٍ التحميل...",
        processing: "جارٍ المعالجة...",
        lengthMenu: "أظهر _MENU_ سجلات",
        zeroRecords: "لم يتم العثور على نتائج",
        info: "إظهار _START_ إلى _END_ من أصل _TOTAL_ سجل",
        infoEmpty: "يعرض 0 إلى 0 من أصل 0 سجل",
        infoFiltered: "(منتقاة من مجموع _MAX_ سجل)",
        search: "ابحث:",
        paginate: {
            first: "الأول",
            last: "الأخير",
            next: "التالي",
            previous: "السابق"
        },
        aria: {
            sortAscending: ": تفعيل لترتيب العمود تصاعدياً",
            sortDescending: ": تفعيل لترتيب العمود تنازلياً"
        }
    };

    // Default configuration
    var config = Object.assign({
        paging: true,
        searching: true,
        ordering: true,
        info: true,
        lengthChange: true,
        responsive: true,
        autoWidth: false,
        language: arabicLang,
    }, options || {});

    return $(selector).DataTable(config);
}