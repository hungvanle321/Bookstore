var dataTable;

$(document).ready(function () {
    GetDataTableData();
});

function GetDataTableData() {
    dataTable = $('#bookTable').DataTable({
        "ajax": { url: '/Admin/Book/GetDataFromAPI' },
        "columns": [
            { data: 'title', "width": "25%"},
            { data: 'author', "width": "15%", className: " text-md-center" },
            { data: 'publisher', "width": "10%"},
            { data: 'language.languageName', "width": "10%"},
            { data: 'discountPrice', "width": "10%"},
            { data: 'category.categoryName', "width": "15%"},
            {
                data: 'bookId',
                "render": function (data) {
                    return `<a href="/admin/book/createandedit?id=${data}" class="btn fs-5 p-0" data-toggle="tooltip" title="Edit this book">
                                <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onClick=CreateAlertForDeleting('/admin/book/delete/${data}') class="btn fs-5" data-toggle="tooltip" title="Delete this book">
                                <i class="bi bi-trash-fill"></i>
                            </a>`
                },
                "width": "15%",
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
function CreateAlertForDeleting(url) {
    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: 'btn btn-success mx-3',
            cancelButton: 'btn btn-danger mx-3'
        },
        buttonsStyling: false
    })

    swalWithBootstrapButtons.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'No, cancel!',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    swalWithBootstrapButtons.fire(
                        'Deleted!',
                        '',
                        'success'
                    )
                }
            })
        } else if (
            /* Read more about handling dismissals below */
            result.dismiss === Swal.DismissReason.cancel
        ) {
            swalWithBootstrapButtons.fire(
                'Cancelled',
                '',
                'error'
            )
        }
    })
}