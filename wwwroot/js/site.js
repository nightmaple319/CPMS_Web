// 全域 JavaScript 功能

$(document).ready(function () {
    // 初始化工具提示
    $('[data-toggle="tooltip"]').tooltip();

    // 初始化彈出提示
    $('[data-toggle="popover"]').popover();

    // 自動隱藏警告訊息
    $('.alert').delay(5000).fadeOut('slow');

    // 確認刪除對話框
    $('.confirm-delete').on('click', function (e) {
        if (!confirm('確定要刪除嗎？此操作無法復原！')) {
            e.preventDefault();
        }
    });

    // 表單提交載入狀態
    $('form').on('submit', function () {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            submitBtn.prop('disabled', true);
            var originalText = submitBtn.html();
            submitBtn.html('<span class="loading"></span> 處理中...');

            // 5秒後恢復按鈕（防止永久卡住）
            setTimeout(function () {
                submitBtn.prop('disabled', false);
                submitBtn.html(originalText);
            }, 5000);
        }
    });

    // 數字輸入格式化
    $('.number-input').on('input', function () {
        var value = $(this).val();
        // 只允許數字和負號
        var numericValue = value.replace(/[^0-9-]/g, '');
        $(this).val(numericValue);
    });

    // 搜尋表單自動提交
    $('.auto-search').on('input', debounce(function () {
        $(this).closest('form').submit();
    }, 500));

    // 表格排序功能
    $('.sortable-table th[data-sort]').on('click', function () {
        var table = $(this).closest('table');
        var column = $(this).data('sort');
        var direction = $(this).hasClass('sort-asc') ? 'desc' : 'asc';

        // 移除其他欄位的排序類別
        table.find('th').removeClass('sort-asc sort-desc');
        $(this).addClass('sort-' + direction);

        // 實際的排序邏輯（需要根據具體需求實作）
        sortTable(table, column, direction);
    });

    // 全選功能
    $('.select-all').on('change', function () {
        var checked = $(this).is(':checked');
        $(this).closest('table').find('.select-item').prop('checked', checked);
        updateBatchActions();
    });

    $('.select-item').on('change', function () {
        updateBatchActions();
    });

    // 批次操作
    function updateBatchActions() {
        var selectedCount = $('.select-item:checked').length;
        $('.batch-actions').toggle(selectedCount > 0);
        $('.selected-count').text(selectedCount);
    }

    // 側邊欄折疊記憶
    if (localStorage.getItem('sidebar-collapsed') === 'true') {
        $('body').addClass('sidebar-collapse');
    }

    $('[data-widget="pushmenu"]').on('click', function () {
        setTimeout(function () {
            var isCollapsed = $('body').hasClass('sidebar-collapse');
            localStorage.setItem('sidebar-collapsed', isCollapsed);
        }, 300);
    });
});

// 防抖函數
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

// 表格排序函數
function sortTable(table, column, direction) {
    var tbody = table.find('tbody');
    var rows = tbody.find('tr').toArray();

    rows.sort(function (a, b) {
        var aVal = $(a).find(`[data-sort-value="${column}"]`).text() ||
            $(a).find('td').eq($(table).find(`th[data-sort="${column}"]`).index()).text();
        var bVal = $(b).find(`[data-sort-value="${column}"]`).text() ||
            $(b).find('td').eq($(table).find(`th[data-sort="${column}"]`).index()).text();

        // 數字排序
        if ($.isNumeric(aVal) && $.isNumeric(bVal)) {
            aVal = parseFloat(aVal);
            bVal = parseFloat(bVal);
        }

        if (direction === 'asc') {
            return aVal > bVal ? 1 : -1;
        } else {
            return aVal < bVal ? 1 : -1;
        }
    });

    tbody.empty().append(rows);
}

// 格式化數字
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// 格式化日期
function formatDate(date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }
    return date.toLocaleDateString('zh-TW');
}

// AJAX 錯誤處理
$(document).ajaxError(function (event, xhr, settings, thrownError) {
    console.error('AJAX Error:', thrownError);

    var message = '操作失敗，請重試！';
    if (xhr.status === 401) {
        message = '登入已過期，請重新登入！';
        window.location.href = '/Identity/Account/Login';
    } else if (xhr.status === 403) {
        message = '沒有權限執行此操作！';
    } else if (xhr.status === 500) {
        message = '伺服器錯誤，請聯絡系統管理員！';
    }

    showNotification(message, 'error');
});

// 通知函數
function showNotification(message, type = 'info') {
    var alertClass = 'alert-info';
    var icon = 'fa-info-circle';

    switch (type) {
        case 'success':
            alertClass = 'alert-success';
            icon = 'fa-check-circle';
            break;
        case 'error':
            alertClass = 'alert-danger';
            icon = 'fa-exclamation-circle';
            break;
        case 'warning':
            alertClass = 'alert-warning';
            icon = 'fa-exclamation-triangle';
            break;
    }

    var notification = $(`
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="fas ${icon} mr-2"></i>
            ${message}
            <button type="button" class="close" data-dismiss="alert">
                <span>&times;</span>
            </button>
        </div>
    `);

    $('.content').prepend(notification);

    // 5秒後自動隱藏
    setTimeout(function () {
        notification.fadeOut();
    }, 5000);
}

// 匯出功能
function exportToExcel(tableId, filename) {
    var table = document.getElementById(tableId);
    var wb = XLSX.utils.table_to_book(table);
    XLSX.writeFile(wb, filename + '.xlsx');
}

// 列印功能
function printTable(tableId) {
    var printWindow = window.open('', '_blank');
    var table = document.getElementById(tableId).outerHTML;

    printWindow.document.write(`
        <html>
        <head>
            <title>列印</title>
            <style>
                table { border-collapse: collapse; width: 100%; }
                th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                th { background-color: #f2f2f2; }
            </style>
        </head>
        <body>${table}</body>
        </html>
    `);

    printWindow.document.close();
    printWindow.print();
}