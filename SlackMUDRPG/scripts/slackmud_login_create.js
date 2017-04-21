"use strict"

$(function () {
	$(".login-form input, .create-form input").on("keyup", function (e) {
		if ($(this).val() != "") {
			$(this).parents(".form-wrapper").find(".alert").slideUp(750, function () {
				$(this).remove();
			});
		}
	});

    $(".tab-login").on("click", function(e) {
        if ($(this).hasClass("active")) {
            return;
        }

        showLogin();
    });

    $(".tab-create").on("click", function(e) {
        if ($(this).hasClass("active")) {
            return;
        }

        showCreate();
    });

    $(".login-link").on("click", function(e) {
        e.preventDefault();

        if ($("#login-tab").hasClass("active")) {
            return;
        }

        showLogin();
    });

    $(".create-link").on("click", function(e) {
        e.preventDefault();

        if ($("#create-tab").hasClass("active")) {
            return;
        }

        showCreate();
    });

    function showLogin() {
        $(".login-form").delay(100).fadeIn(100);
        $(".create-form").fadeOut(100);
        $(".tab-login").addClass("active");
        $(".tab-create").removeClass("active");
    };

    function showCreate() {
        $(".create-form").delay(100).fadeIn(100);
        $(".login-form").fadeOut(100);
        $(".tab-create").addClass("active");
        $(".tab-login").removeClass("active");
    };
});
