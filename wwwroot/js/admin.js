// Toggle the side navigation
document.addEventListener("DOMContentLoaded", function () {
  // Toggle the sidebar
  const sidebarToggle = document.getElementById("sidebarToggle");
  const sidebarToggleTop = document.getElementById("sidebarToggleTop");

  if (sidebarToggle) {
    sidebarToggle.addEventListener("click", function (e) {
      e.preventDefault();
      document.body.classList.toggle("sidebar-toggled");
      document.querySelector(".sidebar").classList.toggle("toggled");
    });
  }

  if (sidebarToggleTop) {
    sidebarToggleTop.addEventListener("click", function (e) {
      e.preventDefault();
      document.body.classList.toggle("sidebar-toggled");
      document.querySelector(".sidebar").classList.toggle("toggled");
    });
  }

  // Close any open menu dropdowns when window is resized
  window.addEventListener("resize", function () {
    const vw = Math.max(
      document.documentElement.clientWidth || 0,
      window.innerWidth || 0
    );

    if (vw < 768) {
      document.body.classList.add("sidebar-toggled");
      document.querySelector(".sidebar").classList.add("toggled");
    } else {
      document.body.classList.remove("sidebar-toggled");
      document.querySelector(".sidebar").classList.remove("toggled");
    }
  });

  // Scroll to top button appear
  const scrollToTop = document.querySelector(".scroll-to-top");

  if (scrollToTop) {
    window.addEventListener("scroll", function () {
      const scrollDistance = window.pageYOffset;

      // Show button when page is scrolled
      if (scrollDistance > 100) {
        scrollToTop.style.display = "block";
      } else {
        scrollToTop.style.display = "none";
      }
    });

    // Scroll to top when button is clicked
    scrollToTop.addEventListener("click", function (e) {
      e.preventDefault();
      window.scrollTo({
        top: 0,
        behavior: "smooth",
      });
    });
  }

  // Initialize tooltips
  const tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Initialize popovers
  const popoverTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="popover"]')
  );
  popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl);
  });
});
