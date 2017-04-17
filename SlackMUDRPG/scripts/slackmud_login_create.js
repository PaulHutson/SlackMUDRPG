"use strict"

$(function () {
    $("#login-tab").on("click", function(e) {
        if ($(this).hasClass("active")) {
            return;
        }

        showLogin();
    });

    $("#create-tab").on("click", function(e) {
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
        $("#login-tab").addClass("active");
        $("#create-tab").removeClass("active");
    };

    function showCreate() {
        $(".create-form").delay(100).fadeIn(100);
        $(".login-form").fadeOut(100);
        $("#create-tab").addClass("active");
        $("#login-tab").removeClass("active");
    };
});
