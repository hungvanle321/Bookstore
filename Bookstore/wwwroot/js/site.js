/** Responsive cart icon's position*/
$(window).on('resize', responsive_change_box_order);

function responsive_change_box_order() {
	if ($(window).width() <= 992) {
		$("#cart-and-login").detach().appendTo("#searchGroup");
	}
	else {
		$("#cart-and-login").detach().appendTo("#togglerGroup");
	}
}

responsive_change_box_order();

/* Search input events */

$(document).ready(function () {
	var suggestMenu = $("nav .nav-item .suggest-menu");
	$('#searchStringInput').keyup(function (event) {
		var keycode = (event.keyCode ? event.keyCode : event.which);
		if (keycode == '13') {
			event.preventDefault();
			$(this).closest('form').submit();
		}
		else {
			var searchText = $('#searchStringInput').val();
			suggestMenu.empty();
			if (searchText.length == 0) {
				suggestMenu.css("display", "none");
				return;
			}
			$.ajax({
				url: '/Customer/Home/SearchAutocomplete',
				data: { "Prefix": searchText },
				type: "POST",
				success: function (data) {
					suggestMenu.css("display", "block");
					if (data.length > 0) {
						$.each(data, function (key, item) {
							var name = item.name;
							var author = item.author;
							var boldPart, formattedName, formattedAuthor;

							if (item.type === 0) {
								boldPart = '<strong>' + name.substring(item.startIndex, item.startIndex + searchText.length) + '</strong>';
								formattedName = name.replace(name.substring(item.startIndex, item.startIndex + searchText.length), boldPart);
								formattedAuthor = author;
							}
							else {
								boldPart = '<strong>' + author.substring(item.startIndex, item.startIndex + searchText.length) + '</strong>';
								formattedAuthor = author.replace(author.substring(item.startIndex, item.startIndex + searchText.length), boldPart);
								formattedName = name;
							}

							var oriPrice = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(item.price1);
							var discountPrice = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(item.price2);
							var searchItem = `
										<a href="/Customer/Home/Details/${item.id}" class="text-decoration-none">
											<div class="suggest-item d-flex py-1">
												<div class="image-section col-2 align-self-center h-100">
													<img src="${item.image}" alt="Book image">
												</div>
												<div class="description-section mx-3">
													<h5>${formattedName}</h5>
													<h6>${formattedAuthor}</h6>
													<div class="d-flex">
														<h6 class="text-decoration-line-through me-2">${oriPrice} </h6>
														<h6 class="fw-bold" style="color: sandybrown;">${discountPrice}</h6>
													</div>
												</div>
											</div>
										</a>
										`;
							suggestMenu.append(searchItem);
						});
					}
					else {
						var searchItem = `
											<div class="w-100 text-center">
												No books found
											</div>
								`;
						suggestMenu.append(searchItem);
					}

				},
				error: function (response) {
					console.log(response.responseText);
				},
				failure: function (response) {
					console.log(response.responseText);
				}
			});
		}
	});

	$('#searchStringInput').focus(function () {
		var searchText = $('#searchStringInput').val();
		if (searchText != "") {
			console.log(0);
			suggestMenu.css("display", "block");
		}
	});

	$(document).on('click', function (event) {
		if (suggestMenu.css('display') === 'block' && !$(event.target).closest('#searchStringInput, nav .nav-item .suggest-menu').length) {
			suggestMenu.css("display", "none");
		}
	});
});




/* User icon events */

$(document).ready(function () {
	$("#menuDropdownButton").click(function () {
		if ($('.navbar-nav .dropdown .dropdown-menu.show').length === 0) {
			$('#userDropdownMenu').addClass('show');
		}
		else {
			$('.navbar-nav .dropdown .dropdown-menu.show').removeClass('show');
		}
	});

	if ($("#managementDropdownItem").length) {
		$("#managementDropdownItem").click(function () {
			$("#userDropdownMenu").removeClass('show');
			$("#managementDropdownMenu").addClass('show');
		});
	}

	if ($("#returnUserMenu").length) {
		$("#returnUserMenu").click(function () {
			$("#userDropdownMenu").addClass('show');
			$("#managementDropdownMenu").removeClass('show');
		});
	}

	$(document).click(function (event) {
		var target = $(event.target);
		var shownDropdownMenu = $('.navbar-nav .dropdown .dropdown-menu.show');

		if (shownDropdownMenu.length && !target.closest('.dropdown').length) {
			shownDropdownMenu.removeClass('show');
		}
	});
});

/* Footer position */

$(document).ready(function () {

	var headerHeight = $('header nav').outerHeight(); // Lấy chiều cao của header
	$('.header-placeholder').css('height', headerHeight + 'px'); // Thiết lập chiều cao của phần tử giả

	checkFooterPosition();

	// Handle window resize event
	var isFirstDataTableCheck = false;
	$(window).resize(function () {
		checkFooterPosition();
	});

	function checkFooterPosition() {
		if ($('.table').length > 0 && isFirstDataTableCheck === false) return;

		var headerHeight = $("header").outerHeight(true);
		var bodyHeight = $('#body-content').outerHeight(true);
		var footerHeight = $('footer').outerHeight(true);
		var windowHeight = $(window).height();

		if (headerHeight + bodyHeight + footerHeight > windowHeight) {
			$("footer").css("position", "relative");
		} else {
			$("footer").css("position", "absolute");
		}
	}

	if ($('.table').length > 0) {
		$('.table').on('draw.dt', function () {
			isFirstDataTableCheck = true;
			checkFooterPosition();
		});
	}
});