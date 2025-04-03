// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Document ready function
document.addEventListener("DOMContentLoaded", function () {
  // Initialize Bootstrap tooltips
  var tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Initialize Bootstrap popovers
  var popoverTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="popover"]')
  );
  var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl);
  });

  // Show job listing cards with animation
  const jobCards = document.querySelectorAll(".job-list-card");
  if (jobCards.length > 0) {
    console.log("Found job cards:", jobCards.length);
    jobCards.forEach((card, index) => {
      // Đảm bảo card luôn hiển thị ngay lập tức - áp dụng style trực tiếp
      card.setAttribute(
        "style",
        "opacity: 1 !important; transform: translateY(0) !important;"
      );

      // Xóa animation classes mà có thể gây ra vấn đề
      card.classList.remove("with-animation", "animate");

      // Thêm class show để đảm bảo hiển thị
      card.classList.add("show");
    });
  }

  // Đảm bảo cards hiển thị khi trang đã load
  window.addEventListener("load", function () {
    const jobCards = document.querySelectorAll(".job-list-card");
    if (jobCards.length > 0) {
      console.log("Ensuring cards visibility on window load");
      jobCards.forEach((card) => {
        // Áp dụng style trực tiếp có tính ưu tiên cao hơn
        card.setAttribute(
          "style",
          "opacity: 1 !important; transform: translateY(0) !important;"
        );

        // Đảm bảo các class animation không còn
        card.classList.remove("with-animation", "animate", "fade-in-up");
        card.classList.add("show");
      });
    }
  });

  // Add staggered animation to items with staggered-item class
  const staggeredItems = document.querySelectorAll(
    ".staggered-animation-container .staggered-item"
  );
  staggeredItems.forEach((item, index) => {
    item.style.animationDelay = `${0.1 * index}s`;
  });

  // Company cards hover effects
  const companyCards = document.querySelectorAll(".company-card");
  if (companyCards.length > 0) {
    companyCards.forEach((card) => {
      // Dùng mouseenter/mouseleave thay vì hover
      card.addEventListener("mouseenter", function (e) {
        // Chỉ áp dụng hover effect cho card này
        this.style.transform = "translateY(-5px)";
        this.style.boxShadow = "0 10px 25px rgba(0, 0, 0, 0.1)";
      });

      card.addEventListener("mouseleave", function (e) {
        // Loại bỏ hover effect khi chuột rời khỏi
        this.style.transform = "translateY(0)";
        this.style.boxShadow =
          "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)";
      });
    });
  }

  // Back to top button
  const backToTopButton = document.getElementById("backToTop");
  if (backToTopButton) {
    window.addEventListener("scroll", function () {
      if (window.pageYOffset > 300) {
        backToTopButton.classList.add("show");
      } else {
        backToTopButton.classList.remove("show");
      }
    });

    backToTopButton.addEventListener("click", function (e) {
      e.preventDefault();
      window.scrollTo({
        top: 0,
        behavior: "smooth",
      });
    });
  }

  // Form validation effects
  const formControls = document.querySelectorAll(".form-control, .form-select");
  formControls.forEach((control) => {
    control.addEventListener("focus", function () {
      this.parentElement.classList.add("focused");
    });

    control.addEventListener("blur", function () {
      this.parentElement.classList.remove("focused");

      // Add was-validated class to the form when a field loses focus
      if (this.value !== "") {
        this.classList.add("is-valid");
      }
    });
  });

  // Password toggle visibility
  const passwordToggles = document.querySelectorAll(".password-toggle");
  passwordToggles.forEach((toggle) => {
    toggle.addEventListener("click", function () {
      const passwordField =
        this.closest(".form-floating").querySelector("input");
      const icon = this.querySelector("i");

      if (passwordField.type === "password") {
        passwordField.type = "text";
        icon.classList.remove("bi-eye-slash");
        icon.classList.add("bi-eye");
      } else {
        passwordField.type = "password";
        icon.classList.remove("bi-eye");
        icon.classList.add("bi-eye-slash");
      }
    });
  });

  // Smooth scroll for anchor links
  const anchorLinks = document.querySelectorAll('a[href^="#"]:not([href="#"])');
  anchorLinks.forEach((link) => {
    link.addEventListener("click", function (e) {
      e.preventDefault();

      const targetId = this.getAttribute("href");
      const targetElement = document.querySelector(targetId);

      if (targetElement) {
        window.scrollTo({
          top: targetElement.offsetTop - 80,
          behavior: "smooth",
        });
      }
    });
  });

  // Add active class to current page in navigation
  const currentLocation = location.pathname;
  const navLinks = document.querySelectorAll(".navbar-nav .nav-link");
  const navLinksLength = navLinks.length;

  for (let i = 0; i < navLinksLength; i++) {
    const linkPath = navLinks[i].getAttribute("href");

    if (
      linkPath === currentLocation ||
      (currentLocation.includes(linkPath) && linkPath !== "/")
    ) {
      navLinks[i].classList.add("active");
    }
  }

  // File input custom text
  const fileInputs = document.querySelectorAll('input[type="file"]');
  fileInputs.forEach((input) => {
    input.addEventListener("change", function (e) {
      const fileName = e.target.files[0]?.name;
      const fileText = this.nextElementSibling;

      if (fileText && fileName) {
        fileText.textContent = fileName;
      }
    });
  });

  // Auto-dismiss alerts after 5 seconds
  const autoAlerts = document.querySelectorAll(
    ".alert.alert-dismissible:not(.alert-permanent)"
  );
  autoAlerts.forEach((alert) => {
    setTimeout(function () {
      const closeButton = alert.querySelector(".btn-close");
      if (closeButton) {
        closeButton.click();
      } else {
        alert.classList.remove("show");
        setTimeout(function () {
          alert.remove();
        }, 300);
      }
    }, 5000);
  });
});
