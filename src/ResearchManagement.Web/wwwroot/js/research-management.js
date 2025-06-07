// Research Management System JavaScript

// Global variables
let currentView = 'table';
let isLoading = false;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeComponents();
    setupEventListeners();
    loadSavedPreferences();
});

// Initialize all components
function initializeComponents() {
    initializeTooltips();
    initializeModals();
    initializeDataTables();
    initializeFormValidation();
    initializeFileUpload();
}

// Setup event listeners
function setupEventListeners() {
    // View toggle buttons
    const viewToggleButtons = document.querySelectorAll('[data-view-toggle]');
    viewToggleButtons.forEach(button => {
        button.addEventListener('click', toggleView);
    });

    // Filter form auto-submit
    const filterInputs = document.querySelectorAll('.auto-filter');
    filterInputs.forEach(input => {
        input.addEventListener('change', debounce(submitFilterForm, 500));
    });

    // Search input with debounce
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(submitFilterForm, 800));
    }

    // Sort buttons
    const sortButtons = document.querySelectorAll('[data-sort]');
    sortButtons.forEach(button => {
        button.addEventListener('click', handleSort);
    });

    // File upload drag and drop
    const fileDropZone = document.getElementById('fileDropZone');
    if (fileDropZone) {
        setupFileDropZone(fileDropZone);
    }

    // Form submission with loading state
    const forms = document.querySelectorAll('form[data-loading]');
    forms.forEach(form => {
        form.addEventListener('submit', handleFormSubmission);
    });

    // Delete confirmations
    const deleteButtons = document.querySelectorAll('[data-delete]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', handleDeleteConfirmation);
    });

    // Status change buttons
    const statusButtons = document.querySelectorAll('[data-status-change]');
    statusButtons.forEach(button => {
        button.addEventListener('click', handleStatusChange);
    });
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Initialize Bootstrap modals
function initializeModals() {
    const modalElements = document.querySelectorAll('.modal');
    modalElements.forEach(modalEl => {
        const modal = new bootstrap.Modal(modalEl);
        
        // Auto-focus first input when modal opens
        modalEl.addEventListener('shown.bs.modal', function() {
            const firstInput = modalEl.querySelector('input, textarea, select');
            if (firstInput) {
                firstInput.focus();
            }
        });
    });
}

// Initialize DataTables if present
function initializeDataTables() {
    const tables = document.querySelectorAll('.data-table');
    tables.forEach(table => {
        if ($.fn.DataTable) {
            $(table).DataTable({
                language: {
                    url: '/lib/datatables/i18n/Arabic.json'
                },
                responsive: true,
                pageLength: 10,
                order: [[0, 'desc']],
                columnDefs: [
                    { orderable: false, targets: -1 } // Last column (actions) not sortable
                ]
            });
        }
    });
}

// Initialize form validation
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                
                // Focus first invalid field
                const firstInvalid = form.querySelector(':invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                }
            }
            form.classList.add('was-validated');
        });
    });
}

// Initialize file upload functionality
function initializeFileUpload() {
    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach(input => {
        input.addEventListener('change', handleFileSelection);
    });
}

// Toggle between table and card views
function toggleView(event) {
    const button = event.currentTarget;
    const targetView = button.getAttribute('data-view-toggle');
    
    const tableView = document.getElementById('tableView');
    const cardView = document.getElementById('cardView');
    
    if (!tableView || !cardView) return;
    
    if (targetView === 'cards') {
        tableView.style.display = 'none';
        cardView.style.display = 'block';
        currentView = 'cards';
        button.innerHTML = '<i class="fas fa-table me-1"></i>عرض جدول';
        button.setAttribute('data-view-toggle', 'table');
    } else {
        tableView.style.display = 'block';
        cardView.style.display = 'none';
        currentView = 'table';
        button.innerHTML = '<i class="fas fa-th-large me-1"></i>عرض بطاقات';
        button.setAttribute('data-view-toggle', 'cards');
    }
    
    // Save preference
    localStorage.setItem('preferredView', currentView);
}

// Handle sorting
function handleSort(event) {
    const button = event.currentTarget;
    const sortField = button.getAttribute('data-sort');
    const currentSort = new URLSearchParams(window.location.search).get('SortBy');
    const currentDesc = new URLSearchParams(window.location.search).get('SortDescending') === 'true';
    
    let newDesc = false;
    if (currentSort === sortField) {
        newDesc = !currentDesc;
    }
    
    // Update URL and reload
    const url = new URL(window.location);
    url.searchParams.set('SortBy', sortField);
    url.searchParams.set('SortDescending', newDesc);
    url.searchParams.set('Page', '1'); // Reset to first page
    
    window.location.href = url.toString();
}

// Submit filter form
function submitFilterForm() {
    const form = document.getElementById('filterForm');
    if (form && !isLoading) {
        isLoading = true;
        showLoading();
        form.submit();
    }
}

// Handle form submission with loading state
function handleFormSubmission(event) {
    const form = event.currentTarget;
    const submitButton = form.querySelector('button[type="submit"]');
    
    if (submitButton) {
        const originalText = submitButton.innerHTML;
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>جاري الحفظ...';
        submitButton.disabled = true;
        
        // Re-enable after 10 seconds as fallback
        setTimeout(() => {
            submitButton.innerHTML = originalText;
            submitButton.disabled = false;
        }, 10000);
    }
}

// Handle delete confirmation
function handleDeleteConfirmation(event) {
    event.preventDefault();
    const button = event.currentTarget;
    const itemName = button.getAttribute('data-item-name') || 'هذا العنصر';
    const deleteUrl = button.getAttribute('data-delete-url') || button.href;
    
    if (confirm(`هل أنت متأكد من حذف ${itemName}؟\nلا يمكن التراجع عن هذا الإجراء.`)) {
        if (deleteUrl) {
            window.location.href = deleteUrl;
        }
    }
}

// Handle status change
function handleStatusChange(event) {
    const button = event.currentTarget;
    const newStatus = button.getAttribute('data-new-status');
    const statusName = button.getAttribute('data-status-name');
    const researchId = button.getAttribute('data-research-id');
    
    if (confirm(`هل تريد تغيير حالة البحث إلى "${statusName}"؟`)) {
        // Show loading
        button.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
        button.disabled = true;
        
        // Make AJAX request
        fetch('/Research/ChangeStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                researchId: researchId,
                newStatus: newStatus
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('تم تغيير الحالة بنجاح', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                showNotification(data.message || 'حدث خطأ أثناء تغيير الحالة', 'error');
            }
        })
        .catch(error => {
            showNotification('حدث خطأ في الاتصال', 'error');
        })
        .finally(() => {
            button.disabled = false;
            // Restore button text would be handled by page reload
        });
    }
}

// Setup file drop zone
function setupFileDropZone(dropZone) {
    dropZone.addEventListener('dragover', function(e) {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });
    
    dropZone.addEventListener('dragleave', function(e) {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
    });
    
    dropZone.addEventListener('drop', function(e) {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
        
        const files = e.dataTransfer.files;
        const fileInput = dropZone.querySelector('input[type="file"]');
        
        if (fileInput && files.length > 0) {
            fileInput.files = files;
            handleFileSelection({ target: fileInput });
        }
    });
}

// Handle file selection
function handleFileSelection(event) {
    const input = event.target;
    const files = input.files;
    const preview = document.getElementById('filePreview');
    
    if (!preview) return;
    
    preview.innerHTML = '';
    
    Array.from(files).forEach((file, index) => {
        const fileItem = createFilePreviewItem(file, index);
        preview.appendChild(fileItem);
    });
    
    // Validate file types and sizes
    validateFiles(files);
}

// Create file preview item
function createFilePreviewItem(file, index) {
    const div = document.createElement('div');
    div.className = 'file-preview-item d-flex align-items-center p-2 border rounded mb-2';
    
    const icon = getFileIcon(file.type);
    const size = formatFileSize(file.size);
    
    div.innerHTML = `
        <i class="${icon} me-2 text-primary"></i>
        <div class="flex-grow-1">
            <div class="fw-bold">${file.name}</div>
            <small class="text-muted">${size}</small>
        </div>
        <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile(${index})">
            <i class="fas fa-times"></i>
        </button>
    `;
    
    return div;
}

// Get file icon based on type
function getFileIcon(mimeType) {
    if (mimeType.includes('pdf')) return 'fas fa-file-pdf text-danger';
    if (mimeType.includes('word')) return 'fas fa-file-word text-primary';
    if (mimeType.includes('excel')) return 'fas fa-file-excel text-success';
    if (mimeType.includes('powerpoint')) return 'fas fa-file-powerpoint text-warning';
    if (mimeType.includes('image')) return 'fas fa-file-image text-info';
    if (mimeType.includes('zip') || mimeType.includes('rar')) return 'fas fa-file-archive text-secondary';
    return 'fas fa-file text-muted';
}

// Format file size
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Validate files
function validateFiles(files) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = [
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-powerpoint',
        'application/vnd.openxmlformats-officedocument.presentationml.presentation'
    ];
    
    let hasErrors = false;
    
    Array.from(files).forEach(file => {
        if (file.size > maxSize) {
            showNotification(`الملف ${file.name} كبير جداً. الحد الأقصى 10 ميجابايت.`, 'warning');
            hasErrors = true;
        }
        
        if (!allowedTypes.includes(file.type)) {
            showNotification(`نوع الملف ${file.name} غير مدعوم.`, 'warning');
            hasErrors = true;
        }
    });
    
    return !hasErrors;
}

// Remove file from selection
function removeFile(index) {
    const fileInput = document.querySelector('input[type="file"]');
    if (!fileInput) return;
    
    const dt = new DataTransfer();
    const files = fileInput.files;
    
    for (let i = 0; i < files.length; i++) {
        if (i !== index) {
            dt.items.add(files[i]);
        }
    }
    
    fileInput.files = dt.files;
    handleFileSelection({ target: fileInput });
}

// Show loading indicator
function showLoading() {
    const loader = document.getElementById('loadingIndicator');
    if (loader) {
        loader.style.display = 'block';
    }
}

// Hide loading indicator
function hideLoading() {
    const loader = document.getElementById('loadingIndicator');
    if (loader) {
        loader.style.display = 'none';
    }
    isLoading = false;
}

// Show notification
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; left: 20px; z-index: 9999; min-width: 300px;';
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Get anti-forgery token
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

// Load saved preferences
function loadSavedPreferences() {
    const savedView = localStorage.getItem('preferredView');
    if (savedView && savedView !== currentView) {
        const toggleButton = document.querySelector(`[data-view-toggle="${savedView}"]`);
        if (toggleButton) {
            toggleButton.click();
        }
    }
}

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Clear all filters
function clearFilters() {
    const form = document.getElementById('filterForm');
    if (!form) return;
    
    // Clear all inputs
    form.querySelectorAll('input[type="text"], input[type="date"], select').forEach(input => {
        input.value = '';
    });
    
    // Reset sort to default
    form.querySelectorAll('input[type="radio"][name="SortBy"]').forEach(radio => {
        radio.checked = radio.value === 'SubmissionDate';
    });
    
    // Submit form
    form.submit();
}

// Export functions for global access
window.ResearchManagement = {
    toggleView,
    clearFilters,
    showNotification,
    handleDeleteConfirmation,
    removeFile,
    formatFileSize
};

// Handle page unload
window.addEventListener('beforeunload', function() {
    hideLoading();
});

// Handle AJAX errors globally
document.addEventListener('ajaxError', function(event) {
    hideLoading();
    showNotification('حدث خطأ في الاتصال. يرجى المحاولة مرة أخرى.', 'error');
});

// Auto-save form data (for long forms)
function setupAutoSave() {
    const forms = document.querySelectorAll('[data-auto-save]');
    forms.forEach(form => {
        const formId = form.id || 'default-form';
        
        // Load saved data
        const savedData = localStorage.getItem(`autosave-${formId}`);
        if (savedData) {
            try {
                const data = JSON.parse(savedData);
                Object.keys(data).forEach(key => {
                    const input = form.querySelector(`[name="${key}"]`);
                    if (input && input.type !== 'file') {
                        input.value = data[key];
                    }
                });
            } catch (e) {
                console.warn('Failed to load auto-saved data:', e);
            }
        }
        
        // Save data on change
        form.addEventListener('input', debounce(() => {
            const formData = new FormData(form);
            const data = {};
            for (let [key, value] of formData.entries()) {
                if (key !== '__RequestVerificationToken') {
                    data[key] = value;
                }
            }
            localStorage.setItem(`autosave-${formId}`, JSON.stringify(data));
        }, 1000));
        
        // Clear saved data on successful submit
        form.addEventListener('submit', () => {
            localStorage.removeItem(`autosave-${formId}`);
        });
    });
}

// Initialize auto-save when DOM is ready
document.addEventListener('DOMContentLoaded', setupAutoSave);