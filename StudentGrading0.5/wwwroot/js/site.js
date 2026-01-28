// Student Grading System - Main JavaScript

// Global app configuration
const app = {
    init: function () {
        this.setupTooltips();
        this.setupAnimations();
        this.setupFormValidation();
        this.setupNotifications();
    },

    // Initialize Bootstrap tooltips
    setupTooltips: function () {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    },

    // Setup scroll animations
    setupAnimations: function () {
        // Fade in elements on scroll
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        }, observerOptions);

        document.querySelectorAll('.card, .stat-card').forEach(el => {
            observer.observe(el);
        });
    },

    // Enhanced form validation
    setupFormValidation: function () {
        const forms = document.querySelectorAll('.needs-validation');

        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();

                    // Show error notification
                    this.showNotification('Please fill in all required fields', 'danger');
                }
                form.classList.add('was-validated');
            }, false);
        });
    },

    // Notification system
    showNotification: function (message, type = 'info') {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
        alertDiv.style.zIndex = '9999';
        alertDiv.innerHTML = `
            <i class="fas fa-${this.getIconForType(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(alertDiv);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    },

    getIconForType: function (type) {
        const icons = {
            'success': 'check-circle',
            'danger': 'exclamation-triangle',
            'warning': 'exclamation-circle',
            'info': 'info-circle'
        };
        return icons[type] || 'info-circle';
    }
};

// Utility Functions
const utils = {
    // Format date
    formatDate: function (dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    // Calculate time remaining
    getTimeRemaining: function (endTime) {
        const total = Date.parse(endTime) - Date.parse(new Date());
        const seconds = Math.floor((total / 1000) % 60);
        const minutes = Math.floor((total / 1000 / 60) % 60);
        const hours = Math.floor((total / (1000 * 60 * 60)) % 24);

        return {
            total,
            hours,
            minutes,
            seconds
        };
    },

    // Debounce function for search inputs
    debounce: function (func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Copy text to clipboard
    copyToClipboard: function (text) {
        navigator.clipboard.writeText(text).then(() => {
            app.showNotification('Copied to clipboard!', 'success');
        }).catch(err => {
            console.error('Failed to copy:', err);
            app.showNotification('Failed to copy', 'danger');
        });
    }
};

// Exam-specific functions
const examModule = {
    // Auto-save exam answers
    autoSave: function (examId) {
        const textareas = document.querySelectorAll('textarea[name^="answers"]');

        textareas.forEach(textarea => {
            textarea.addEventListener('input', utils.debounce(() => {
                const key = `exam_${examId}_${textarea.name}`;
                localStorage.setItem(key, textarea.value);

                // Show saved indicator
                this.showSaveIndicator(textarea);
            }, 1000));
        });
    },

    showSaveIndicator: function (element) {
        const indicator = document.createElement('small');
        indicator.className = 'text-success ms-2';
        indicator.innerHTML = '<i class="fas fa-check-circle"></i> Saved';

        const existingIndicator = element.parentElement.querySelector('.text-success');
        if (existingIndicator) {
            existingIndicator.remove();
        }

        element.parentElement.appendChild(indicator);

        setTimeout(() => {
            indicator.remove();
        }, 2000);
    },

    // Clear saved answers
    clearSavedAnswers: function (examId) {
        const keys = Object.keys(localStorage).filter(key => key.startsWith(`exam_${examId}`));
        keys.forEach(key => localStorage.removeItem(key));
    }
};

// Statistics and Chart helpers
const statsModule = {
    // Calculate average
    average: function (array) {
        return array.reduce((a, b) => a + b, 0) / array.length;
    },

    // Get grade color
    getGradeColor: function (grade) {
        const colors = {
            5: '#11998e',
            4: '#667eea',
            3: '#f2994a',
            2: '#eb3349',
            1: '#969696'
        };
        return colors[grade] || '#969696';
    },

    // Format percentage
    formatPercentage: function (value) {
        return `${value.toFixed(1)}%`;
    }
};

// Search and Filter functionality
const searchModule = {
    // Filter table rows
    filterTable: function (searchTerm, tableId) {
        const table = document.getElementById(tableId);
        const rows = table.querySelectorAll('tbody tr');

        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            const matches = text.includes(searchTerm.toLowerCase());
            row.style.display = matches ? '' : 'none';
        });
    },

    // Sort table
    sortTable: function (columnIndex, tableId, ascending = true) {
        const table = document.getElementById(tableId);
        const tbody = table.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        rows.sort((a, b) => {
            const aValue = a.cells[columnIndex].textContent.trim();
            const bValue = b.cells[columnIndex].textContent.trim();

            const comparison = aValue.localeCompare(bValue, undefined, { numeric: true });
            return ascending ? comparison : -comparison;
        });

        rows.forEach(row => tbody.appendChild(row));
    }
};

// Loading overlay
const loadingModule = {
    show: function (message = 'Loading...') {
        const overlay = document.createElement('div');
        overlay.id = 'loadingOverlay';
        overlay.className = 'spinner-overlay';
        overlay.innerHTML = `
            <div class="text-center">
                <div class="spinner-border text-light" style="width: 3rem; height: 3rem;" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-white mt-3">${message}</p>
            </div>
        `;
        document.body.appendChild(overlay);
    },

    hide: function () {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.remove();
        }
    }
};

// Initialize app when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    app.init();

    // Log app initialization
    console.log('Student Grading System initialized');
});

// Expose modules globally
window.StudentGradingApp = {
    app,
    utils,
    examModule,
    statsModule,
    searchModule,
    loadingModule
};

