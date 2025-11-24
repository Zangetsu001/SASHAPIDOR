document.addEventListener('DOMContentLoaded', function () {
    const headerTop = document.querySelector('.header-top');
    const headerImageContainer = document.querySelector('.header-image-container');
    const headerTopHeight = headerTop.offsetHeight;

    window.addEventListener('scroll', function () {
        const scrollPosition = window.scrollY;

        if (scrollPosition > headerTopHeight) {
            headerTop.style.backgroundColor = '#007bff';
            headerTop.style.borderBottom = '1px solid #e07b00';
        } else {
            headerTop.style.backgroundColor = 'transparent';
            headerTop.style.borderBottom = 'none';
        }
    });
});