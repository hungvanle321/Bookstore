var dataTable;

$(document).ready(function () {
    GetDataTableData();
});

function GetDataTableData() {
    dataTable = $('#bookTable').DataTable({
            "ajax": { url: '/Admin/Book/GetDataFromAPI' },
            "columns": [
                { data: 'title', "width": "35%" },
                { data: 'authors', "width": "20%" },
                { data: 'isbn', "width": "10%" }, 
                { data: 'price', "width": "5%" },
                { data: 'category.name', "width":"20%" },
                {
                    data: 'id',
                    "render": function (data) {
                        return `<a href="/admin/book/createandedit?id=${data}" class="btn btn-dark mx-1" data-toggle="tooltip" title="Edit">
								    <i class="bi bi-pencil-square"></i>
							    </a>
							    <a onClick=CreateAlertForDeleting('/admin/book/delete/${data}') class="btn btn-primary" data-toggle="tooltip" title="Delete">
								    <i class="bi bi-trash-fill"></i>
							    </a>`
                    },
                    "width": "10%"
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