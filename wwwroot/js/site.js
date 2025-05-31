// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getRelativeTime(date) {
  const now = new Date();
  const seconds = Math.floor((now - date) / 1000);
  if (seconds < 5) return "Just now";
  if (seconds < 60) return `${seconds} seconds ago`;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes} minute${minutes > 1 ? "s" : ""} ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hour${hours > 1 ? "s" : ""} ago`;
  const days = Math.floor(hours / 24);
  return `${days} day${days > 1 ? "s" : ""} ago`;
}

function showToast(message, type = "info", duration = 5000) {
  const toastId = `toast-${Date.now()}`;
  const barId = `${toastId}-bar`;

  const config = {
    success: {
      bg: "bg-success text-white",
      title: "Success",
      icon: "bi-check-circle-fill",
      border: "bg-success",
      bgbtn: "btn-close-white",
    },
    error: {
      bg: "bg-danger text-white",
      title: "Error",
      icon: "bi-x-circle-fill",
      border: "bg-danger",
      bgbtn: "btn-close-white",
    },
    info: {
      bg: "bg-primary text-white",
      title: "Info",
      icon: "bi-info-circle-fill",
      border: "bg-primary",
      bgbtn: "btn-close-white",
    },
    warning: {
      bg: "bg-warning text-dark",
      title: "Warning",
      icon: "bi-exclamation-triangle-fill",
      border: "bg-warning",
      bgbtn: "",
    },
  };

  const { bg, title, icon, border, bgbtn } = config[type] || config.info;
  const timeAgo = getRelativeTime(new Date());

  const toastHtml = `
  <div class="toast fade border position-relative animate-in shadow-md" role="alert" aria-live="assertive" aria-atomic="true" id="${toastId}">
    <div class="toast-header ${bg} py-2">
      <i class="bi ${icon} me-2 align-middle"></i>
      <strong class="me-auto align-middle">${title}</strong>
    <small class="ms-2 fs-6 align-middle ${type === "warning" ? "text-dark" : "text-white"}">${timeAgo}</small>
      <button type="button" class="btn-close ${bgbtn} ms-2" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
    <div class="fs-6 toast-body py-3">
      ${message}
    </div>
    <div id="${barId}" class="${border}" style="
      position: absolute;
      bottom: 0;
      left: 0;
      height: 4px;
      width: 100%;
      transition: width ${duration}ms linear;
      border-bottom-left-radius: 0.375rem;
      border-bottom-right-radius: 0.375rem;
    "></div>
  </div>
`;

  const container = document.getElementById("toastContainer");
  container.insertAdjacentHTML("beforeend", toastHtml);

  const toastEl = document.getElementById(toastId);
  const barEl = document.getElementById(barId);
  const toast = new bootstrap.Toast(toastEl, { delay: duration });

  setTimeout(() => {
    if (barEl) barEl.style.width = "0%";
  }, 50);

  toastEl.addEventListener("mouseenter", () => {
    barEl.style.transition = "none";
  });

  toastEl.addEventListener("mouseleave", () => {
    const elapsed = (1 - parseFloat(barEl.style.width) / 100) * duration;
    barEl.style.transition = `width ${elapsed}ms linear`;
    barEl.style.width = "0%";
  });

  toastEl.addEventListener("hide.bs.toast", () => {
    toastEl.classList.remove("animate-in");
    toastEl.classList.add("animate-out");
  });

  toastEl.addEventListener("hidden.bs.toast", () => {
    setTimeout(() => toastEl.remove(), 400);
  });

  toast.show();
}
