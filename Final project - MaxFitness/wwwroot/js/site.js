// ============================================================================
// MaxFitness - Site JavaScript
// Modern ES6+ | Vanilla JS | No jQuery
// ============================================================================

document.addEventListener("DOMContentLoaded", () => {

    // ========================================================================
    // Utility Functions
    // ========================================================================

    /**
     * Debounce function to limit how often a callback fires.
     * @param {Function} fn - The function to debounce.
     * @param {number} delay - Delay in milliseconds.
     * @returns {Function}
     */
    const debounce = (fn, delay = 15) => {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn(...args), delay);
        };
    };

    /**
     * EaseOutExpo easing function for smooth counter animations.
     * @param {number} t - Progress from 0 to 1.
     * @returns {number}
     */
    const easeOutExpo = (t) => (t === 1 ? 1 : 1 - Math.pow(2, -10 * t));

    /**
     * Format a number with commas (e.g. 12345 -> "12,345").
     * @param {number} n
     * @returns {string}
     */
    const formatNumberWithCommas = (n) => {
        return Math.floor(n).toLocaleString("en-US");
    };

    /**
     * Create a shared IntersectionObserver for one-time reveal-style triggers.
     * @param {Function} callback - Called with (entry) when element enters view.
     * @param {object} options - IntersectionObserver options.
     * @returns {IntersectionObserver}
     */
    const createOnceObserver = (callback, options = {}) => {
        const defaults = { threshold: 0.1, rootMargin: "0px" };
        const merged = { ...defaults, ...options };

        return new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    callback(entry);
                    observer.unobserve(entry.target);
                }
            });
        }, merged);
    };


    // ========================================================================
    // 1. Navbar Scroll Effect
    // ========================================================================

    const initNavbarScroll = () => {
        const navbar = document.querySelector(".navbar-maxfit");
        if (!navbar) return;

        const SCROLL_THRESHOLD = 20;

        const handleScroll = () => {
            if (window.scrollY > SCROLL_THRESHOLD) {
                navbar.classList.add("scrolled");
            } else {
                navbar.classList.remove("scrolled");
            }
        };

        // Check initial state on load
        handleScroll();

        window.addEventListener("scroll", debounce(handleScroll, 10), { passive: true });
    };


    // ========================================================================
    // 2. Scroll Reveal Animations
    // ========================================================================

    const initScrollReveal = () => {
        const revealElements = document.querySelectorAll(
            ".reveal, .reveal-up, .reveal-left, .reveal-right, .reveal-scale"
        );

        if (revealElements.length === 0) return;

        const revealObserver = createOnceObserver((entry) => {
            const el = entry.target;

            // If the element has stagger children, animate them with delays
            if (el.classList.contains("reveal-stagger")) {
                const children = el.querySelectorAll(
                    ".reveal, .reveal-up, .reveal-left, .reveal-right, .reveal-scale"
                );
                children.forEach((child, index) => {
                    setTimeout(() => {
                        child.classList.add("revealed");
                    }, index * 120);
                });
            }

            el.classList.add("revealed");
        }, { threshold: 0.1 });

        revealElements.forEach((el) => revealObserver.observe(el));
    };


    // ========================================================================
    // 3. Interactive Body Map
    // ========================================================================

    const initBodyMap = () => {
        const muscleGroups = document.querySelectorAll(".muscle-group");
        if (muscleGroups.length === 0) return;

        // --- Panel elements ---
        const infoPanel = document.getElementById("muscle-info-panel");
        const muscleNameEl = document.getElementById("muscle-name");
        const muscleStatusEl = document.getElementById("muscle-status");
        const muscleLastTrainedEl = document.getElementById("muscle-last-trained");
        const muscleExercisesEl = document.getElementById("muscle-exercises");

        // --- Status color map ---
        const statusColors = {
            strong: {
                fill: "rgba(34, 197, 94, 0.3)",
                stroke: "#22c55e",
                badgeClass: "status-strong",
            },
            moderate: {
                fill: "rgba(255, 107, 53, 0.3)",
                stroke: "#ff6b35",
                badgeClass: "status-moderate",
            },
            "needs-work": {
                fill: "rgba(230, 57, 70, 0.3)",
                stroke: "#e63946",
                badgeClass: "status-needs-work",
            },
        };

        // --- Apply initial colors based on data-status ---
        muscleGroups.forEach((group) => {
            const status = group.dataset.status;
            const colors = statusColors[status];
            if (colors) {
                group.style.fill = colors.fill;
                group.style.stroke = colors.stroke;
                group.style.strokeWidth = "1.5";
                group.style.transition = "fill 0.3s ease, stroke 0.3s ease, opacity 0.3s ease";
                group.style.cursor = "pointer";
            }
        });

        // --- Tooltip element ---
        let tooltip = document.querySelector(".muscle-tooltip");
        if (!tooltip) {
            tooltip = document.createElement("div");
            tooltip.className = "muscle-tooltip";
            tooltip.style.position = "absolute";
            tooltip.style.pointerEvents = "none";
            tooltip.style.opacity = "0";
            tooltip.style.transition = "opacity 0.2s ease";
            tooltip.style.zIndex = "1000";
            document.body.appendChild(tooltip);
        }

        /**
         * Update the side info panel with muscle data.
         */
        const updateInfoPanel = (group) => {
            if (!infoPanel) return;

            const name = group.dataset.muscle || "Unknown";
            const status = group.dataset.status || "unknown";
            const lastTrained = group.dataset.lastTrained || "N/A";
            const exercises = group.dataset.exercises || "0";
            const colors = statusColors[status];

            if (muscleNameEl) {
                muscleNameEl.textContent = name;
            }

            if (muscleStatusEl) {
                // Remove old status classes, add new one
                muscleStatusEl.className = "muscle-status-badge";
                if (colors) {
                    muscleStatusEl.classList.add(colors.badgeClass);
                }
                muscleStatusEl.textContent =
                    status.charAt(0).toUpperCase() + status.slice(1).replace("-", " ");
            }

            if (muscleLastTrainedEl) {
                muscleLastTrainedEl.textContent = lastTrained;
            }

            if (muscleExercisesEl) {
                muscleExercisesEl.textContent = `${exercises} exercise${exercises !== "1" ? "s" : ""}`;
            }

            // Show the panel
            infoPanel.classList.remove("hidden");
            infoPanel.style.display = "";
        };

        /**
         * Show the cursor tooltip near the mouse.
         */
        const showTooltip = (group, e) => {
            const name = group.dataset.muscle || "Unknown";
            const status = group.dataset.status || "unknown";
            const label = status.charAt(0).toUpperCase() + status.slice(1).replace("-", " ");

            tooltip.innerHTML = `<strong>${name}</strong><br><span>${label}</span>`;
            tooltip.style.opacity = "1";

            positionTooltip(e);
        };

        const positionTooltip = (e) => {
            const offsetX = 15;
            const offsetY = 15;
            const tooltipRect = tooltip.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;

            let left = e.pageX + offsetX;
            let top = e.pageY + offsetY;

            // Prevent tooltip from overflowing the right edge
            if (left + tooltipRect.width > viewportWidth + window.scrollX) {
                left = e.pageX - tooltipRect.width - offsetX;
            }

            // Prevent tooltip from overflowing the bottom edge
            if (top + tooltipRect.height > viewportHeight + window.scrollY) {
                top = e.pageY - tooltipRect.height - offsetY;
            }

            tooltip.style.left = `${left}px`;
            tooltip.style.top = `${top}px`;
        };

        const hideTooltip = () => {
            tooltip.style.opacity = "0";
        };

        // --- Event listeners for each muscle group ---
        muscleGroups.forEach((group) => {
            // Hover enter
            group.addEventListener("mouseenter", (e) => {
                group.classList.add("active");
                updateInfoPanel(group);
                showTooltip(group, e);
            });

            // Mouse move (reposition tooltip)
            group.addEventListener("mousemove", (e) => {
                positionTooltip(e);
            });

            // Hover leave - keep the panel showing the last hovered muscle (better UX)
            group.addEventListener("mouseleave", () => {
                group.classList.remove("active");
                hideTooltip();
            });

            // Click - toggle selected/targeted state
            group.addEventListener("click", () => {
                group.classList.toggle("targeted");
            });
        });
    };


    // ========================================================================
    // 4. Animated Counters
    // ========================================================================

    const initCounters = () => {
        const counters = document.querySelectorAll(".counter-value[data-target]");
        if (counters.length === 0) return;

        const DURATION = 2000; // 2 seconds

        const animateCounter = (el) => {
            const target = parseFloat(el.dataset.target) || 0;
            const startTime = performance.now();

            const step = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / DURATION, 1);
                const easedProgress = easeOutExpo(progress);
                const currentValue = easedProgress * target;

                el.textContent = formatNumberWithCommas(currentValue);

                if (progress < 1) {
                    requestAnimationFrame(step);
                } else {
                    // Ensure final value is exact
                    el.textContent = formatNumberWithCommas(target);
                }
            };

            requestAnimationFrame(step);
        };

        const counterObserver = createOnceObserver((entry) => {
            animateCounter(entry.target);
        }, { threshold: 0.3 });

        counters.forEach((counter) => counterObserver.observe(counter));
    };


    // ========================================================================
    // 5. Progress Bar Animation
    // ========================================================================

    const initProgressBars = () => {
        const progressBars = document.querySelectorAll(".progress-fill");
        if (progressBars.length === 0) return;

        // Set initial width to 0 so we can animate to target
        progressBars.forEach((bar) => {
            // Store the target width from data attribute or inline style
            const targetWidth =
                bar.dataset.width ||
                bar.style.width ||
                "0%";
            bar.dataset.targetWidth = targetWidth;
            bar.style.width = "0";
            bar.style.transition = "width 1.5s ease-out";
        });

        const progressObserver = createOnceObserver((entry) => {
            const bar = entry.target;
            const targetWidth = bar.dataset.targetWidth || "0%";

            // Use requestAnimationFrame to ensure the browser processes the 0-width first
            requestAnimationFrame(() => {
                bar.style.width = targetWidth;
            });
        }, { threshold: 0.1 });

        progressBars.forEach((bar) => progressObserver.observe(bar));
    };


    // ========================================================================
    // 6. Weekly Chart Bar Animation
    // ========================================================================

    const initChartBars = () => {
        const chartBars = document.querySelectorAll(".chart-bar");
        if (chartBars.length === 0) return;

        // Store target heights and reset to 0
        chartBars.forEach((bar) => {
            const targetHeight =
                bar.dataset.height ||
                bar.style.height ||
                "0%";
            bar.dataset.targetHeight = targetHeight;
            bar.style.height = "0";
            bar.style.transition = "height 0.8s ease-out";
        });

        // We need a container-level observer so we can stagger the children.
        // Find unique parent containers of chart bars.
        const observedParents = new Set();

        chartBars.forEach((bar) => {
            const parent = bar.parentElement;
            if (parent && !observedParents.has(parent)) {
                observedParents.add(parent);
            }
        });

        if (observedParents.size > 0) {
            const chartObserver = createOnceObserver((entry) => {
                const container = entry.target;
                const bars = container.querySelectorAll(".chart-bar");

                bars.forEach((bar, index) => {
                    setTimeout(() => {
                        requestAnimationFrame(() => {
                            bar.style.height = bar.dataset.targetHeight || "0%";
                        });
                    }, index * 100);
                });
            }, { threshold: 0.1 });

            observedParents.forEach((parent) => chartObserver.observe(parent));
        } else {
            // Fallback: observe individual bars if no common parent found
            const chartObserver = createOnceObserver((entry) => {
                const bar = entry.target;
                requestAnimationFrame(() => {
                    bar.style.height = bar.dataset.targetHeight || "0%";
                });
            }, { threshold: 0.1 });

            chartBars.forEach((bar) => chartObserver.observe(bar));
        }
    };


    // ========================================================================
    // 7. Mobile Menu Toggle
    // ========================================================================

    const initMobileMenu = () => {
        const toggler = document.querySelector(".navbar-toggler");
        const navCollapse = document.querySelector(".navbar-collapse");
        if (!toggler || !navCollapse) return;

        // Track state
        let isOpen = false;

        // Set up initial collapsed styles for smooth animation
        if (!navCollapse.classList.contains("show")) {
            navCollapse.style.maxHeight = "0";
            navCollapse.style.overflow = "hidden";
            navCollapse.style.transition = "max-height 0.35s ease";
        }

        const openMenu = () => {
            navCollapse.classList.add("show", "menu-animating");
            navCollapse.style.maxHeight = navCollapse.scrollHeight + "px";
            toggler.setAttribute("aria-expanded", "true");
            isOpen = true;

            const onTransitionEnd = () => {
                navCollapse.classList.remove("menu-animating");
                navCollapse.removeEventListener("transitionend", onTransitionEnd);
            };
            navCollapse.addEventListener("transitionend", onTransitionEnd);
        };

        const closeMenu = () => {
            navCollapse.style.maxHeight = "0";
            navCollapse.classList.add("menu-animating");
            toggler.setAttribute("aria-expanded", "false");
            isOpen = false;

            const onTransitionEnd = () => {
                navCollapse.classList.remove("show", "menu-animating");
                navCollapse.removeEventListener("transitionend", onTransitionEnd);
            };
            navCollapse.addEventListener("transitionend", onTransitionEnd);
        };

        // Toggle button click
        toggler.addEventListener("click", (e) => {
            e.stopPropagation();
            if (isOpen) {
                closeMenu();
            } else {
                openMenu();
            }
        });

        // Close menu when clicking a nav link
        const navLinks = navCollapse.querySelectorAll("a.nav-link");
        navLinks.forEach((link) => {
            link.addEventListener("click", () => {
                if (isOpen) {
                    closeMenu();
                }
            });
        });

        // Close menu when clicking outside
        document.addEventListener("click", (e) => {
            if (isOpen && !navCollapse.contains(e.target) && !toggler.contains(e.target)) {
                closeMenu();
            }
        });
    };


    // ========================================================================
    // 8. Smooth Scroll for Anchor Links
    // ========================================================================

    const initSmoothScroll = () => {
        const anchorLinks = document.querySelectorAll('a[href^="#"]');
        if (anchorLinks.length === 0) return;

        const getNavbarHeight = () => {
            const navbar = document.querySelector(".navbar-maxfit");
            return navbar ? navbar.offsetHeight : 0;
        };

        anchorLinks.forEach((link) => {
            link.addEventListener("click", (e) => {
                const href = link.getAttribute("href");

                // Skip if it is just "#" or empty
                if (!href || href === "#") return;

                const target = document.querySelector(href);
                if (!target) return;

                e.preventDefault();

                const navbarOffset = getNavbarHeight();
                const elementPosition = target.getBoundingClientRect().top + window.scrollY;
                const offsetPosition = elementPosition - navbarOffset - 20;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: "smooth",
                });

                // Update URL hash without jumping
                if (history.pushState) {
                    history.pushState(null, null, href);
                }
            });
        });
    };


    // ========================================================================
    // 9. Parallax Subtle Effect
    // ========================================================================

    const initParallax = () => {
        const heroSection = document.querySelector(".hero-section, .hero, [data-parallax]");
        if (!heroSection) return;

        const parallaxElements = heroSection.querySelectorAll(
            ".hero-bg, .hero-shape, .hero-decoration, [data-parallax-speed]"
        );

        // If no specific parallax children, apply to the section background
        const targets =
            parallaxElements.length > 0
                ? Array.from(parallaxElements)
                : [heroSection];

        let ticking = false;
        let lastScrollY = 0;

        const updateParallax = () => {
            const scrollY = lastScrollY;
            const heroBottom = heroSection.offsetTop + heroSection.offsetHeight;

            // Only apply parallax when the hero section is in view
            if (scrollY > heroBottom) {
                ticking = false;
                return;
            }

            targets.forEach((el) => {
                const speed = parseFloat(el.dataset.parallaxSpeed) || 0.15;
                const yOffset = -(scrollY * speed);
                el.style.transform = `translate3d(0, ${yOffset}px, 0)`;
            });

            ticking = false;
        };

        const onScroll = () => {
            lastScrollY = window.scrollY;
            if (!ticking) {
                requestAnimationFrame(updateParallax);
                ticking = true;
            }
        };

        window.addEventListener("scroll", onScroll, { passive: true });
    };


    // ========================================================================
    // 10. Tooltip for Body Map
    //     The tooltip logic is integrated into Section 3 (initBodyMap) above.
    //     The tooltip is created dynamically if .muscle-tooltip does not exist,
    //     positioned near the cursor on mousemove, and hidden on mouseleave.
    // ========================================================================


    // ========================================================================
    // Initialize All Modules
    // ========================================================================

    const init = () => {
        initNavbarScroll();
        initScrollReveal();
        initBodyMap();
        initCounters();
        initProgressBars();
        initChartBars();
        initMobileMenu();
        initSmoothScroll();
        initParallax();
    };

    init();

});
