/* ==========================================================================
   RaceDay — Part 2 Prototype
   Shared, dependency-free JavaScript.
   Each feature below is guarded by a feature check so the same file can be
   included safely on every page.
   ========================================================================== */
(function () {
  "use strict";

  /* ----------------------------- DOM helpers ----------------------------- */
  function $(selector, scope) { return (scope || document).querySelector(selector); }
  function $$(selector, scope) { return Array.prototype.slice.call((scope || document).querySelectorAll(selector)); }
  function on(el, evt, fn) { if (el) { el.addEventListener(evt, fn); } }

  /* --------------------------- Mobile nav toggle ------------------------- */
  function initNavToggle() {
    var toggle = $("#navToggle");
    var nav = $("#primaryNav");
    if (!toggle || !nav) { return; }

    on(toggle, "click", function () {
      var isOpen = nav.classList.toggle("open");
      toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    });

    // Close the menu when a link is chosen (mobile convenience)
    $$("a", nav).forEach(function (link) {
      on(link, "click", function () {
        if (window.matchMedia("(max-width: 760px)").matches) {
          nav.classList.remove("open");
          toggle.setAttribute("aria-expanded", "false");
        }
      });
    });
  }

  /* ------------------------------ Toast helper --------------------------- */
  function getToastContainer() {
    var container = $(".toast-container");
    if (!container) {
      container = document.createElement("div");
      container.className = "toast-container";
      container.setAttribute("role", "status");
      container.setAttribute("aria-live", "polite");
      document.body.appendChild(container);
    }
    return container;
  }

  var TOAST_ICONS = { success: "✅", warning: "⚠️", danger: "⛔", info: "ℹ️", default: "🔔" };

  function showToast(title, message, type) {
    type = type || "info";
    var container = getToastContainer();
    var toast = document.createElement("div");
    toast.className = "toast toast-" + type;
    toast.setAttribute("role", "status");

    var icon = document.createElement("span");
    icon.className = "toast-icon";
    icon.setAttribute("aria-hidden", "true");
    icon.textContent = TOAST_ICONS[type] || TOAST_ICONS["default"];

    var body = document.createElement("div");
    var t = document.createElement("p");
    t.className = "toast-title";
    t.textContent = title;
    body.appendChild(t);
    if (message) {
      var m = document.createElement("p");
      m.className = "toast-msg";
      m.textContent = message;
      body.appendChild(m);
    }

    var close = document.createElement("button");
    close.type = "button";
    close.className = "toast-close";
    close.setAttribute("aria-label", "Dismiss notification");
    close.textContent = "✕";

    toast.appendChild(icon);
    toast.appendChild(body);
    toast.appendChild(close);
    container.appendChild(toast);

    function remove() {
      if (toast.parentNode) { toast.parentNode.removeChild(toast); }
    }
    on(close, "click", remove);
    window.setTimeout(remove, 6000);
    return toast;
  }

  // Expose for inline handlers / other pages
  window.showToast = showToast;

  /* ------------------------- Event filtering (index) --------------------- */
  function initEventFilters() {
    var search = $("#filterSearch");
    var type = $("#filterType");
    var location = $("#filterLocation");
    var upcoming = $("#filterUpcoming");
    var grid = $("#eventGrid");
    if (!grid) { return; }

    var cards = $$(".event-card", grid);
    var emptyState = $("#noResults");

    function applyFilters() {
      var q = (search && search.value ? search.value : "").trim().toLowerCase();
      var typeVal = type ? type.value : "all";
      var locVal = location ? location.value : "all";
      var onlyUpcoming = upcoming ? upcoming.checked : false;
      var visible = 0;

      cards.forEach(function (card) {
        var name = (card.getAttribute("data-name") || "").toLowerCase();
        var cardType = card.getAttribute("data-type") || "";
        var cardLoc = card.getAttribute("data-location") || "";
        var isUpcoming = card.getAttribute("data-upcoming") === "true";

        var matches = true;
        if (q && name.indexOf(q) === -1) { matches = false; }
        if (typeVal !== "all" && cardType !== typeVal) { matches = false; }
        if (locVal !== "all" && cardLoc !== locVal) { matches = false; }
        if (onlyUpcoming && !isUpcoming) { matches = false; }

        card.classList.toggle("hidden", !matches);
        if (matches) { visible++; }
      });

      if (emptyState) { emptyState.classList.toggle("hidden", visible > 0); }
      var count = $("#resultsCount");
      if (count) {
        count.textContent = visible + (visible === 1 ? " event found" : " events found");
      }
    }

    on(search, "input", applyFilters);
    on(search, "change", applyFilters);
    on(type, "change", applyFilters);
    on(location, "change", applyFilters);
    on(upcoming, "change", applyFilters);

    var reset = $("#filterReset");
    on(reset, "click", function () {
      if (search) { search.value = ""; }
      if (type) { type.value = "all"; }
      if (location) { location.value = "all"; }
      if (upcoming) { upcoming.checked = false; }
      applyFilters();
    });

    applyFilters();
  }

  /* ---------------------------- Form validation -------------------------- */
  function setFieldError(input, message) {
    var group = input.closest(".form-group") || input.parentNode;
    if (!group) { return; }
    group.classList.add("field-error");
    var msg = group.querySelector(".error-msg");
    if (msg) {
      msg.textContent = message;
      msg.setAttribute("role", "alert");
    }
    input.setAttribute("aria-invalid", "true");
  }

  function clearFieldError(input) {
    var group = input.closest(".form-group") || input.parentNode;
    if (!group) { return; }
    group.classList.remove("field-error");
    var msg = group.querySelector(".error-msg");
    if (msg) { msg.textContent = ""; }
    input.removeAttribute("aria-invalid");
  }

  var EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  var MIN_PASSWORD = 8;

  function validateRequired(input) {
    if (!input.value.trim()) { setFieldError(input, "This field is required."); return false; }
    clearFieldError(input); return true;
  }

  function validateEmail(input) {
    if (!input.value.trim()) { setFieldError(input, "Email is required."); return false; }
    if (!EMAIL_RE.test(input.value.trim())) { setFieldError(input, "Enter a valid email address."); return false; }
    clearFieldError(input); return true;
  }

  function validatePassword(input) {
    if (!input.value) { setFieldError(input, "Password is required."); return false; }
    if (input.value.length < MIN_PASSWORD) { setFieldError(input, "Password must be at least " + MIN_PASSWORD + " characters."); return false; }
    clearFieldError(input); return true;
  }

  // Clear the error as soon as the user corrects the field
  function attachLiveClear(form) {
    $$("input, select, textarea", form).forEach(function (input) {
      on(input, "input", function () { clearFieldError(input); });
      on(input, "change", function () { clearFieldError(input); });
    });
  }

  /* ------------------------------ Login form ----------------------------- */
  function initLoginForm() {
    var form = $("#loginForm");
    if (!form) { return; }
    attachLiveClear(form);

    var email = $("#loginEmail");
    var password = $("#loginPassword");

    on(form, "submit", function (e) {
      e.preventDefault();
      var okEmail = validateEmail(email);
      var okPw = validatePassword(password);
      if (!okEmail || !okPw) {
        showToast("Check your details", "Please correct the highlighted fields.", "warning");
        (!okEmail ? email : password).focus();
        return;
      }

      var role = ($("input[name='role']:checked", form) || {}).value || "Participant";
      form.classList.add("hidden");
      var success = $("#loginSuccess");
      if (success) {
        success.classList.remove("hidden");
        var roleSpan = $("#successRole");
        if (roleSpan) { roleSpan.textContent = role; }
        var greeting = $("#successGreeting");
        if (greeting) { greeting.textContent = "Signed in as " + email.value.trim(); }
      }
      showToast("Welcome back!", "You are signed in as " + role + ".", "success");
    });
  }

  /* ---------------------------- Register form ---------------------------- */
  function initRegisterForm() {
    var form = $("#registerForm");
    if (!form) { return; }
    attachLiveClear(form);

    var first = $("#regFirstName");
    var last = $("#regLastName");
    var email = $("#regEmail");
    var password = $("#regPassword");
    var confirm = $("#regConfirm");

    on(form, "submit", function (e) {
      e.preventDefault();
      var ok = true;
      ok = validateRequired(first) && ok;
      ok = validateRequired(last) && ok;
      ok = validateEmail(email) && ok;
      ok = validatePassword(password) && ok;

      if (!confirm.value) { setFieldError(confirm, "Please confirm your password."); ok = false; }
      else if (confirm.value !== password.value) { setFieldError(confirm, "Passwords do not match."); ok = false; }
      else { clearFieldError(confirm); }

      if (!ok) {
        showToast("Almost there", "Please fix the highlighted fields to continue.", "warning");
        return;
      }

      form.classList.add("hidden");
      var success = $("#registerSuccess");
      if (success) { success.classList.remove("hidden"); }
      showToast("Account created", "Your RaceDay account is ready. You can now log in.", "success");
    });
  }

  /* ----------------------------- Profile form ---------------------------- */
  function initProfileForm() {
    var form = $("#profileForm");
    if (!form) { return; }

    // Visual-only avatar upload preview
    var avatarInput = $("#avatarInput");
    var avatarImg = $("#avatarPreview");
    on(avatarInput, "change", function () {
      var file = avatarInput.files && avatarInput.files[0];
      if (!file) { return; }
      var reader = new FileReader();
      reader.onload = function (ev) {
        if (avatarImg) { avatarImg.src = ev.target.result; }
        showToast("Photo updated", "Remember to save your changes.", "info");
      };
      reader.readAsDataURL(file);
    });

    on(form, "submit", function (e) {
      e.preventDefault();
      var email = $("#profileEmail");
      if (email && !validateEmail(email)) {
        showToast("Check your details", "Enter a valid email address.", "warning");
        return;
      }
      var success = $("#profileSaved");
      if (success) {
        success.classList.remove("hidden");
        success.focus();
      }
      showToast("Changes saved", "Your profile has been updated.", "success");
    });
  }

  /* --------------------- Event detail: enter/reserve --------------------- */
  function initEnrolBehaviour() {
    var form = $("#enrolForm");
    if (!form) { return; }

    var button = $("#enterEventBtn");
    var radios = $$("input[name='category']", form);

    on(button, "click", function (e) {
      e.preventDefault();
      var selected = $("input[name='category']:checked", form);
      if (!selected) {
        showToast("Select a category", "Choose a race category before entering the event.", "warning");
        if (radios[0]) { radios[0].focus(); }
        return;
      }

      var card = selected.closest(".category-row");
      var name = card ? card.getAttribute("data-category-name") : "your category";

      $$(".category-row", form).forEach(function (row) { row.classList.remove("is-enrolled"); });
      if (card) {
        card.classList.add("is-enrolled");
        var flag = card.querySelector(".enrolled-flag");
        if (flag) { flag.classList.remove("hidden"); }
      }

      button.disabled = true;
      button.textContent = "Enrolled ✓";
      button.classList.remove("btn-rd-primary");
      button.classList.add("btn-rd-dark");

      var status = $("#enrolStatus");
      if (status) {
        status.classList.remove("hidden");
        status.textContent = "Your entry for " + name + " is pending confirmation. " +
          "You can view it under My Events.";
      }

      showToast("Entry submitted", "You have entered " + name + ". Status: Pending.", "success");
    });
  }

  /* ------------------- Organiser: create/edit event form ----------------- */
  function initEventForm() {
    var form = $("#eventForm");
    if (!form) { return; }
    attachLiveClear(form);

    // Banner upload preview
    var bannerInput = $("#bannerInput");
    var previewWrap = $("#bannerPreview");
    on(bannerInput, "change", function () {
      var file = bannerInput.files && bannerInput.files[0];
      if (!file || !previewWrap) { return; }
      var reader = new FileReader();
      reader.onload = function (ev) {
        previewWrap.innerHTML = "";
        var img = document.createElement("img");
        img.src = ev.target.result;
        img.alt = "Preview of the event banner image you selected";
        previewWrap.appendChild(img);
        showToast("Banner added", "Preview updated with your new banner image.", "info");
      };
      reader.readAsDataURL(file);
    });

    // Add category row
    var addBtn = $("#addCategoryBtn");
    var catBody = $("#categoryTableBody");
    on(addBtn, "click", function (e) {
      e.preventDefault();
      if (!catBody) { return; }
      var row = document.createElement("tr");
      row.innerHTML =
        '<td><label class="visually-hidden" for="newCatName">Category name</label>' +
        '<input type="text" id="newCatName" placeholder="e.g. Open Women"></td>' +
        '<td><label class="visually-hidden" for="newCatAge">Age group</label>' +
        '<input type="text" id="newCatAge" placeholder="e.g. 18-39"></td>' +
        '<td><label class="visually-hidden" for="newCatDist">Distance</label>' +
        '<input type="text" id="newCatDist" placeholder="e.g. 10km"></td>' +
        '<td><label class="visually-hidden" for="newCatFee">Entry fee</label>' +
        '<input type="text" id="newCatFee" placeholder="e.g. R180"></td>' +
        '<td><label class="visually-hidden" for="newCatSpaces">Spaces</label>' +
        '<input type="number" id="newCatSpaces" min="0" placeholder="e.g. 250"></td>';
      catBody.appendChild(row);
      var firstInput = row.querySelector("input");
      if (firstInput) { firstInput.focus(); }
      showToast("Category added", "Complete the details for the new category.", "info");
    });

    on(form, "submit", function (e) {
      e.preventDefault();
      var name = $("#eventName");
      if (name && !validateRequired(name)) {
        showToast("Check the form", "An event name is required.", "warning");
        name.focus();
        return;
      }
      showToast("Event saved", "Your event details have been saved (prototype).", "success");
    });
  }

  /* -------------------- Participant: cancel an enrolment ----------------- */
  function initCancelActions() {
    $$("[data-action='cancel']").forEach(function (btn) {
      on(btn, "click", function (e) {
        e.preventDefault();
        var row = btn.closest("tr") || btn.closest(".event-card");
        if (!row) { return; }
        var badge = row.querySelector(".badge");
        if (badge) {
          badge.className = "badge badge-cancelled";
          badge.textContent = "Cancelled";
        }
        var trigger = row.querySelector("[data-action='cancel']");
        if (trigger) { trigger.remove(); }
        var restore = document.createElement("span");
        restore.className = "muted";
        restore.textContent = "—";
        var cell = btn.closest("td");
        if (cell) { cell.appendChild(restore); }
        showToast("Enrolment cancelled", "Your entry has been cancelled.", "danger");
      });
    });
  }

  /* ------------------- Organiser: publish results action ----------------- */
  function initResultsActions() {
    var publish = $("#publishResultsBtn");
    on(publish, "click", function (e) {
      e.preventDefault();
      var inputs = $$(".finish-time-input, .finish-position-input");
      var filled = inputs.filter(function (i) { return i.value.trim() !== ""; }).length;
      showToast("Results published", filled + " result fields published to participants.", "success");
    });

    var saveDraft = $("#saveDraftBtn");
    on(saveDraft, "click", function (e) {
      e.preventDefault();
      showToast("Draft saved", "Your results entry has been saved as a draft.", "info");
    });
  }

  /* --------------------------- Enrolment actions ------------------------- */
  function initEnrolmentActions() {
    $$("[data-action='approve']").forEach(function (btn) {
      on(btn, "click", function (e) {
        e.preventDefault();
        var row = btn.closest("tr");
        var badge = row && row.querySelector(".badge");
        if (badge) { badge.className = "badge badge-confirmed"; badge.textContent = "Confirmed"; }
        showToast("Enrolment confirmed", "The participant has been confirmed.", "success");
      });
    });
  }

  /* -------------------------------- Init --------------------------------- */
  function init() {
    initNavToggle();
    initEventFilters();
    initLoginForm();
    initRegisterForm();
    initProfileForm();
    initEnrolBehaviour();
    initEventForm();
    initCancelActions();
    initResultsActions();
    initEnrolmentActions();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
