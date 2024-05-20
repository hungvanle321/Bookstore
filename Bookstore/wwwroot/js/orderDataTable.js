var dataTable;

$(document).ready(function () {
    GetDataTableData();
});

function GetDataTableData() {
    dataTable = $('#orderTable').DataTable({
        "ajax": { url: '/Admin/Order/GetDataFromAPI' },
        "columns": [
            { data: 'applicationUser.name', "width": "20%" },
            { data: 'applicationUser.userName', "width": "15%" },
            {
                data: 'orderDate',
                "render": function (data) {
                    var date = new Date(data);
                    var month = date.getMonth() + 1;
                    var day = date.getDate();
                    var year = date.getFullYear();
                    return (month < 10 ? '0' : '') + month + '/' + (day < 10 ? '0' : '') + day + '/' + year;
                },
                "width": "15%",
                className: "text-center"
            },
            { data: 'orderStatus', "width": "15%", className: "text-center" },
            { data: 'paymentStatus', "width": "15%", className: "text-center" },
            {
                data: 'orderTotal',
                "render": function (data) {
                    return '$' + parseFloat(data).toFixed(2);
                },
                "width": "10%"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<a href="/admin/order/edit?id=${data}" class="btn fs-5 p-0" data-toggle="tooltip" title="Edit this order">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                            `
                },
                "width": "10%",
                className: " text-center",
                "orderable": false
            }
        ],
        "columnDefs": [
            {
                "className": "align-middle",
                "targets": "_all"
            }
        ]
    });
}
