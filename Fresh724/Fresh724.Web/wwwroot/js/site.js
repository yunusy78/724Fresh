// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


$( document ).ready(function() {
    console.log( "ready!" );

    // Vanilla Javascript
    var input = document.querySelector("#Phone");
    window.intlTelInput(input,({
        // options here
    }));
});




var multipleCardCarousel = document.querySelector(
    "#carouselExampleControls"
);
if (window.matchMedia("(min-width: 768px)").matches) {
    var carousel = new bootstrap.Carousel(multipleCardCarousel, {
        interval: false,
    });
    var carouselWidth = $(".carousel-inner")[0].scrollWidth;
    var cardWidth = $(".carousel-item").width();
    var scrollPosition = 0;
    $("#carouselExampleControls .carousel-control-next").on("click", function () {
        if (scrollPosition < carouselWidth - cardWidth * 4) {
            scrollPosition += cardWidth;
            $("#carouselExampleControls .carousel-inner").animate(
                { scrollLeft: scrollPosition },
                600
            );
        }
    });
    $("#carouselExampleControls .carousel-control-prev").on("click", function () {
        if (scrollPosition > 0) {
            scrollPosition -= cardWidth;
            $("#carouselExampleControls .carousel-inner").animate(
                { scrollLeft: scrollPosition },
                600
            );
        }
    });
} else {
    $(multipleCardCarousel).addClass("slide");
}


var multipleCardCarousel1 = document.querySelector(
    "#carouselExampleControls1"
);
if (window.matchMedia("(min-width: 768px)").matches) {
    var carouse = new bootstrap.Carousel(multipleCardCarousel1, {
        interval: false,
    });
    var carouselWidth1 = $(".carousel-inner")[0].scrollWidth;
    var cardWidth1 = $(".carousel-item").width();
    var scrollPosition1 = 0;
    $("#carouselExampleControls1 .carousel-control-next").on("click", function () {
        if (scrollPosition1 < carouselWidth1 - cardWidth1 * 4) {
            scrollPosition1 += cardWidth1;
            $("#carouselExampleControls1 .carousel-inner").animate(
                { scrollLeft: scrollPosition1 },
                600
            );
        }
    });
    $("#carouselExampleControls1 .carousel-control-prev").on("click", function () {
        if (scrollPosition1 > 0) {
            scrollPosition1 -= cardWidth1;
            $("#carouselExampleControls1 .carousel-inner").animate(
                { scrollLeft: scrollPosition1 },
                600
            );
        }
    });
} else {
    $(multipleCardCarousel1).addClass("slide");
}



// Vanilla Javascript


