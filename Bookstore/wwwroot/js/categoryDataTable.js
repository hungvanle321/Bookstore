var dataTable;

$(document).ready(function () {
    GetDataTableData();
});

function GetDataTableData() {
    dataTable = $('#categoryTable').DataTable({
        "ajax": { url: '/Admin/Category/GetDataFromAPI' },
        "columns": [
            { data: 'categoryName', "width": "35%" },
            {
                data: 'categoryId',
                "render": function (data, type, row) {
                    return `<a onClick="CreateAlertForEditing('/admin/category/edit/${data}', '${row.categoryName}')" class="btn btn-dark mx-1" data-toggle="tooltip" title="Edit">
								    <i class="bi bi-pencil-square"></i>
							    </a>
							    <a onClick="CreateAlertForDeleting('/admin/category/delete/${data}')" class="btn btn-primary" data-toggle="tooltip" title="Delete">
								    <i class="bi bi-trash-fill"></i>
							    </a>`
                },
                "width": "10%",
                className: "text-center"
            }
        ]
    });
}

function CreateAlertForDeleting(url) {
    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            cancelButton: 'btn btn-danger mx-3',
            confirmButton: 'btn btn-success mx-3'
            
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
                    if (data.success) {
                        swalWithBootstrapButtons.fire(
                            'Deleted!',
                            data.message, // Access the message from the response
                            'success'
                        );
                    } else {
                        swalWithBootstrapButtons.fire(
                            'Error',
                            data.message, // Access the message from the response
                            'error'
                        );
                    }
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

function CreateAlertForAdding(url) {
    Swal.fire({
        title: 'Enter name:',
        input: 'text',
        inputAttributes: {
            autocapitalize: 'off'
        },
        showCancelButton: true,
        confirmButtonText: 'Submit',
        showLoaderOnConfirm: true,
        preConfirm: (value) => {
            if (!value) {
                Swal.showValidationMessage('Input cannot be empty.');
                return false;
            }

            return $.ajax({
                url: url,
                type: 'POST',
                data: { inputValue: value }, 
            })
                .done(function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                })
                .fail(function () {
                    Swal.fire('Error', 'An error occurred while processing your request.', 'error');
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {

        if (result.isConfirmed) {
            dataTable.ajax.reload();
        }
    });
}

function CreateAlertForEditing(url, initialValue) {
    Swal.fire({
        title: 'Enter name:',
        input: 'text',
        inputAttributes: {
            autocapitalize: 'off'
        },
        inputValue: initialValue,
        showCancelButton: true,
        confirmButtonText: 'Submit',
        showLoaderOnConfirm: true,
        preConfirm: (value) => {
            if (!value) {
                Swal.showValidationMessage('Input cannot be empty.');
                return false;
            }

            return $.ajax({
                url: url,
                type: 'PUT',
                data: { inputValue: value },
            })
                .done(function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                })
                .fail(function () {
                    Swal.fire('Error', 'An error occurred while processing your request.', 'error');
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {

        if (result.isConfirmed) {
            dataTable.ajax.reload();
        }
    });
}