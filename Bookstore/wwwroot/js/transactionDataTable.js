var dataTable;

$(document).ready(function () {
    GetDataTableData();
});

function GetDataTableData() {
    dataTable = $('#transactionTable').DataTable({
        "ajax": { url: '/Admin/Transaction/GetDataFromAPI' },
        "columns": [
            {
                "render": function (data, type, row) {
                    return "Order ID: " + row.orderHeader.id + "<br>Transaction Code: " + row.transactionCode;
                },
                "width": "20%",
                "title": "Order ID/Transaction Code"
            },
            {
                "render": function (data, type, row) {
                    return "Name: " + row.orderHeader.shipName + "<br>Email: " + row.orderHeader.shipEmail + "<br>Phone: " + row.orderHeader.shipPhoneNumber;
                },
                "width": "20%",
                "title": "Customer"
            },
            {
                "render": function (data, type, row) {
                    return "Name: " + row.buyerName + "<br>Method: " + row.method + "<br>Card Number: " + row.cardNumber + "<br>Bank Name: " + row.bankName;
                },
                "width": "20%",
                "title": "Card"
            },
            {
                "render": function (data, type, row) {
                    return "Amount: $" + (row.amount / 24640).toFixed(2) + "<br>Payer Fee: $" + (row.payerFee / 24640).toFixed(2);
                },
                "width": "15%",
                "title": "Amount"
            },
            {
                "render": function (data, type, row) {
                    var successTime = new Date(row.successTime);
                    var formattedTime = ("0" + successTime.getDate()).slice(-2) + "/" + ("0" + (successTime.getMonth() + 1)).slice(-2) + "/" + successTime.getFullYear() + " - " + ("0" + successTime.getHours()).slice(-2) + ":" + ("0" + successTime.getMinutes()).slice(-2) + ":" + ("0" + successTime.getSeconds()).slice(-2);
                    return formattedTime;
                },
                "width": "10%",
                "title": "Success Time"
            },
            {
                "render": function (data, type, row) {
                    return row.status === "000" ? "Completed" : "Rejected";
                },
                "width": "10%",
                "title": "Status"
            },
            {
                data: 'transactionCode',
                "render": function (data) {
                    return `<a href="/admin/transaction/ShowTransaction?id=${data}" class="btn fs-5 p-0" data-toggle="tooltip" title="View this transaction">
                                <i class="bi bi-info-circle"></i>
                            </a>
                            `
                },
                "width": "5%",
                className: " text-center",
                "orderable": false
            }
        ]
    });
}
