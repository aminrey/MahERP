// ========================================
// ⭐⭐⭐ Tooltip Initialization Helper
// ========================================
function initializeTooltips(container) {
    // ⭐ Initialize custom tooltips
    if (typeof CustomTooltip !== 'undefined') {
        try {
            const count = CustomTooltip.init('[data-tooltip]', {
                delay: 150,
                hideDelay: 50
            });
            if (count > 0) {
                console.log(`✅ ${count} custom tooltips initialized`);
            }
        } catch (error) {
            console.warn('⚠️ Error initializing tooltips:', error);
        }
    } else {
        console.warn('⚠️ CustomTooltip not loaded');
    }
}

// ========================================
// ⭐⭐⭐ State Management
// ========================================
const TaskListState = {
    currentViewType: 0,
    currentGrouping: 0,
    currentStatusFilter: 1,
    currentFilters: {},
    hasAdvancedFilters: false,

    update: function (newState) {
        if (newState.currentViewType !== undefined) {
            this.currentViewType = newState.currentViewType;
        }
        if (newState.currentGrouping !== undefined) {
            this.currentGrouping = newState.currentGrouping;
        }
        if (newState.currentStatusFilter !== undefined) {
            this.currentStatusFilter = newState.currentStatusFilter;
        }
        if (newState.currentFilters !== undefined) {
            this.currentFilters = newState.currentFilters;
            this.hasAdvancedFilters = Object.keys(newState.currentFilters).length > 0;
        }

        console.log('📊 State Updated:', {
            viewType: this.currentViewType,
            grouping: this.currentGrouping,
            statusFilter: this.currentStatusFilter,
            hasAdvancedFilters: this.hasAdvancedFilters
        });
    },

    getState: function () {
        return {
            viewType: this.currentViewType,
            grouping: this.currentGrouping,
            statusFilter: this.currentStatusFilter,
            currentFilters: this.currentFilters
        };
    }
};

// ========================================
// ⭐⭐⭐ Group Collapse Manager
// ========================================
const GroupCollapseManager = {
    storageKey: 'taskGroupsCollapseState',

    saveState: function (groupKey, isCollapsed) {
        const state = this.getState();
        state[groupKey] = isCollapsed;
        localStorage.setItem(this.storageKey, JSON.stringify(state));
    },

    getState: function () {
        const saved = localStorage.getItem(this.storageKey);
        return saved ? JSON.parse(saved) : {};
    },

    getGroupState: function (groupKey) {
        const state = this.getState();
        return state[groupKey] !== undefined ? state[groupKey] : false;
    },

    clearState: function () {
        localStorage.removeItem(this.storageKey);
    },

    applyState: function () {
        const state = this.getState();

        $('.block-rounded.block-bordered').each(function () {
            const $block = $(this);
            const groupTitle = $block.find('.block-title').text().trim();
            const groupKey = 'group_' + groupTitle.replace(/\s+/g, '_');

            const isCollapsed = state[groupKey] || false;

            if (isCollapsed) {
                $block.find('.block-content').hide();
                $block.find('.btn-block-option i').removeClass('si-arrow-up').addClass('si-arrow-down');
            } else {
                $block.find('.block-content').show();
                $block.find('.btn-block-option i').removeClass('si-arrow-down').addClass('si-arrow-up');
            }
        });
    },

    expandAll: function () {
        $('.block-rounded.block-bordered').each(function () {
            const $block = $(this);
            const groupTitle = $block.find('.block-title').text().trim();
            const groupKey = 'group_' + groupTitle.replace(/\s+/g, '_');

            $block.find('.block-content').slideDown(300);
            $block.find('.btn-block-option i').removeClass('si-arrow-down').addClass('si-arrow-up');

            GroupCollapseManager.saveState(groupKey, false);
        });
    },

    collapseAll: function () {
        $('.block-rounded.block-bordered').each(function () {
            const $block = $(this);
            const groupTitle = $block.find('.block-title').text().trim();
            const groupKey = 'group_' + groupTitle.replace(/\s+/g, '_');

            $block.find('.block-content').slideUp(300);
            $block.find('.btn-block-option i').removeClass('si-arrow-up').addClass('si-arrow-down');

            GroupCollapseManager.saveState(groupKey, true);
        });
    }
};

function initializeGroupToggle() {
    $(document).off('click', '.btn-block-option').on('click', '.btn-block-option', function (e) {
        e.preventDefault();

        const $btn = $(this);
        const $block = $btn.closest('.block');
        const $content = $block.find('.block-content');
        const $icon = $btn.find('i');

        const groupTitle = $block.find('.block-title').text().trim();
        const groupKey = 'group_' + groupTitle.replace(/\s+/g, '_');

        if ($content.is(':visible')) {
            $content.slideUp(300);
            $icon.removeClass('si-arrow-up').addClass('si-arrow-down');
            GroupCollapseManager.saveState(groupKey, true);
        } else {
            $content.slideDown(300);
            $icon.removeClass('si-arrow-down').addClass('si-arrow-up');
            GroupCollapseManager.saveState(groupKey, false);
        }
    });
}

// ========================================
// ⭐⭐⭐ Task List Manager
// ========================================
const TaskListManager = {
    config: null,

    init: function (config) {
        this.config = config || window.TaskListConfig;

        if (!this.config) {
            console.error('❌ TaskListConfig not found!');
            return;
        }

        console.log('✅ TaskListManager initialized');
    },

    updateStats: function (stats) {
        if (!stats) return;

        $('.quick-status-filter').each(function () {
            const filter = $(this).data('filter');
            const $value = $(this).find('.fs-3');

            if (filter === 1) $value.text(stats.pending || 0);
            else if (filter === 2) $value.text(stats.completed || 0);
            else if (filter === 3) $value.text(stats.overdue || 0);
            else if (filter === 4) $value.text(stats.urgent || 0);
        });
    },

    changeGrouping: function (grouping) {
        const viewType = TaskListState.currentViewType;

        console.log(`🔄 Changing grouping to: ${grouping}, viewType: ${viewType}`);

        $('#task-groups-container').html('<div class="text-center p-5"><i class="fa fa-spinner fa-spin fa-3x text-primary"></i><p class="mt-3">در حال بارگذاری...</p></div>');

        $.ajax({
            url: this.config.urls.changeGrouping,
            type: 'POST',
            data: {
                viewType: viewType,
                grouping: grouping,
                currentFilters: TaskListState.currentFilters,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                console.log('✅ Grouping changed:', response);

                if (response.status === 'update-view' && response.viewList && response.viewList.length > 0) {
                    response.viewList.forEach(function (item) {
                        if (item.elementId && item.view && item.view.result) {
                            $('#' + item.elementId).html(item.view.result);

                            setTimeout(function () {
                                GroupCollapseManager.applyState();
                                initializeGroupToggle();
                                // ⭐⭐⭐ Initialize tooltips after content load
                                initializeTooltips(document.getElementById(item.elementId));
                            }, 100);

                            if (typeof ModalUtils !== 'undefined') {
                                ModalUtils.processUrlsInContainer($('#' + item.elementId));
                            }
                        }
                    });

                    TaskListState.update({
                        currentGrouping: response.currentGrouping || grouping,
                        currentViewType: response.currentViewType || viewType
                    });

                    $('.btn-grouping').removeClass('btn-primary').addClass('btn-alt-secondary');
                    $(`.btn-grouping[data-grouping="${grouping}"]`).removeClass('btn-alt-secondary').addClass('btn-primary');

                    if (response.stats) {
                        TaskListManager.updateStats(response.stats);
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ Error:', error);
                $('#task-groups-container').html('<div class="alert alert-danger">خطا در تغییر گروه‌بندی</div>');
            }
        });
    },

    changeViewType: function (viewType) {
        if (TaskListState.currentViewType === viewType) {
            return;
        }

        window.location.href = `/TaskingArea/Tasks/Index?viewType=${viewType}&grouping=${TaskListState.currentGrouping}`;
    }
};

// ========================================
// ⭐⭐⭐ Advanced Filters Manager
// ========================================
const AdvancedFiltersManager = {
    initialized: false,
    filtersLoaded: false,

    init: function () {
        if (this.initialized) {
            return;
        }

        $(document).off('click.toggleAdvancedFilters', '#toggle-advanced-filters-btn');
        $(document).on('click.toggleAdvancedFilters', '#toggle-advanced-filters-btn', function (e) {
            e.preventDefault();

            const $panel = $('#advanced-filters-panel');
            const $btn = $('#toggle-advanced-filters-btn');

            $panel.slideToggle(300, function () {
                if ($panel.is(':visible')) {
                    $btn.addClass('active');
                    $btn.find('i').removeClass('fa-filter').addClass('fa-minus');

                    if (!AdvancedFiltersManager.filtersLoaded) {
                        AdvancedFiltersManager.loadFilterData();
                    }
                } else {
                    $btn.removeClass('active');
                    $btn.find('i').removeClass('fa-minus').addClass('fa-filter');
                }
            });
        });

        $(document).off('submit.advancedFilters', '#advanced-filters-form');
        $(document).on('submit.advancedFilters', '#advanced-filters-form', function (e) {
            e.preventDefault();
            AdvancedFiltersManager.applyFilters();
        });

        $(document).off('click.clearFilters', '#clear-filters-btn');
        $(document).on('click.clearFilters', '#clear-filters-btn', function (e) {
            e.preventDefault();
            AdvancedFiltersManager.clearFilters();
        });

        this.initialized = true;
    },

    loadFilterData: function () {
        if (this.filtersLoaded) return;

        $.ajax({
            url: '/TaskingArea/Tasks/GetAdvancedFilterData',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.status === 'success') {
                    const $creatorSelect = $('#creator-user-select');
                    $creatorSelect.empty().append('<option value="">همه</option>');
                    if (response.users && response.users.length) {
                        response.users.forEach(function (user) {
                            $creatorSelect.append($('<option></option>').val(user.id).text(user.fullName));
                        });
                    }

                    const $assignedSelect = $('#assigned-user-select');
                    $assignedSelect.empty().append('<option value="">همه</option>');
                    if (response.users && response.users.length) {
                        response.users.forEach(function (user) {
                            $assignedSelect.append($('<option></option>').val(user.id).text(user.fullName));
                        });
                    }

                    const $organizationSelect = $('#organization-select');
                    $organizationSelect.empty().append('<option value="">همه سازمان‌ها</option>');
                    if (response.organizations && response.organizations.length) {
                        response.organizations.forEach(function (org) {
                            const displayText = org.name + (org.type ? ` (${org.type})` : '');
                            $organizationSelect.append($('<option></option>').val(org.id).text(displayText));
                        });
                    }

                    const $teamSelect = $('#team-select');
                    $teamSelect.empty().append('<option value="">همه تیم‌ها</option>');
                    if (response.teams && response.teams.length) {
                        response.teams.forEach(function (team) {
                            const displayText = team.title + (team.managerName ? ` (مدیر: ${team.managerName})` : '');
                            $teamSelect.append($('<option></option>').val(team.id).text(displayText));
                        });
                    }

                    const $categorySelect = $('#category-select');
                    $categorySelect.empty().append('<option value="">همه</option>');
                    if (response.categories && response.categories.length) {
                        response.categories.forEach(function (category) {
                            $categorySelect.append($('<option></option>').val(category.id).text(category.title));
                        });
                    }

                    this.filtersLoaded = true;
                }
            }.bind(this)
        });
    },

    applyFilters: function () {
        const formData = $('#advanced-filters-form').serializeArray();
        const filters = {};

        formData.forEach(function (field) {
            if (field.value && field.value !== '') {
                filters[field.name] = field.value;
            }
        });

        filters.ViewType = TaskListState.currentViewType;
        filters.Grouping = TaskListState.currentGrouping;

        const $submitBtn = $('#advanced-filters-form button[type="submit"]');
        const originalText = $submitBtn.html();
        $submitBtn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin me-1"></i> در حال اعمال...');

        $.ajax({
            url: '/TaskingArea/Tasks/ApplyAdvancedFilters',
            type: 'POST',
            data: filters,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.status === 'update-view' && response.viewList) {
                    response.viewList.forEach(function (item) {
                        if (item.elementId && item.view && item.view.result) {
                            $('#' + item.elementId).html(item.view.result);

                            setTimeout(function () {
                                GroupCollapseManager.applyState();
                                initializeGroupToggle();
                                // ⭐⭐⭐ Initialize tooltips after content load
                                initializeTooltips(document.getElementById(item.elementId));
                            }, 100);
                        }
                    });
                }

                TaskListState.update({
                    currentFilters: filters,
                    currentViewType: response.currentViewType || TaskListState.currentViewType,
                    currentGrouping: response.currentGrouping || TaskListState.currentGrouping
                });

                if (response.stats) {
                    TaskListManager.updateStats(response.stats);
                }
            },
            complete: function () {
                $submitBtn.prop('disabled', false).html(originalText);
            }
        });
    },

    clearFilters: function () {
        $('#advanced-filters-form')[0].reset();

        $.ajax({
            url: '/TaskingArea/Tasks/ClearAdvancedFilters',
            type: 'POST',
            data: {
                viewType: TaskListState.currentViewType,
                grouping: TaskListState.currentGrouping,
                statusFilter: TaskListState.currentStatusFilter
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.status === 'update-view' && response.viewList) {
                    response.viewList.forEach(function (item) {
                        if (item.elementId && item.view && item.view.result) {
                            $('#' + item.elementId).html(item.view.result);

                            setTimeout(function () {
                                GroupCollapseManager.applyState();
                                initializeGroupToggle();
                                // ⭐⭐⭐ Initialize tooltips after content load
                                initializeTooltips(document.getElementById(item.elementId));
                            }, 100);
                        }
                    });
                }

                TaskListState.update({
                    currentFilters: {},
                    currentViewType: response.currentViewType || TaskListState.currentViewType,
                    currentGrouping: response.currentGrouping || TaskListState.currentGrouping,
                    currentStatusFilter: response.currentStatusFilter || TaskListState.currentStatusFilter
                });

                if (response.stats) {
                    TaskListManager.updateStats(response.stats);
                }
            }
        });
    }
};

// ========================================
// ⭐⭐⭐ Global Functions
// ========================================
function changeGrouping(grouping) {
    TaskListManager.changeGrouping(grouping);
}

function changeViewType(viewType) {
    TaskListManager.changeViewType(viewType);
}
// ========================================
// ⭐⭐⭐ Document Ready
// ========================================
$(document).ready(function () {
    console.log('🔄 TaskList.js: Document Ready');

    // ⭐⭐⭐ مقداردهی اولیه State از window.TaskListInitialState
    if (typeof window.TaskListInitialState !== 'undefined') {
        TaskListState.update(window.TaskListInitialState);
        console.log('✅ TaskListState initialized:', TaskListState.getState());
    } else {
        console.error('❌ window.TaskListInitialState not found!');
    }

    if (typeof window.TaskListConfig === 'undefined') {
        console.error('❌ TaskListConfig not found!');
        return;
    }

    TaskListManager.init(window.TaskListConfig);
    AdvancedFiltersManager.init();

    GroupCollapseManager.applyState();
    initializeGroupToggle();
    
    // ⭐⭐⭐ Initialize tooltips on page load
    initializeTooltips();

    // ⭐⭐⭐ Event handler برای دکمه‌های گروه‌بندی
    $(document).on('click', '.btn-grouping', function (e) {
        e.preventDefault();
        const grouping = parseInt($(this).data('grouping'));
        console.log('🎯 Grouping button clicked:', grouping);
        changeGrouping(grouping);
    });

    // ⭐⭐⭐ Event handler برای expand/collapse
    $(document).on('click', '#expand-all-groups', function () {
        GroupCollapseManager.expandAll();
    });

    $(document).on('click', '#collapse-all-groups', function () {
        GroupCollapseManager.collapseAll();
    });

    // ⭐⭐⭐ Event handler برای کلیک روی آمار سریع
    $(document).on('click', '.quick-status-filter', function (e) {
        e.preventDefault();

        const $this = $(this);
        const statusFilter = parseInt($this.data('filter'));

        console.log('🎯 Quick Status Filter clicked:', statusFilter);
        console.log('📊 Current State:', TaskListState.getState());

        $('#task-groups-container').html('<div class="text-center p-5"><i class="fa fa-spinner fa-spin fa-3x text-primary"></i><p class="mt-3">در حال بارگذاری...</p></div>');

        $.ajax({
            url: TaskListManager.config.urls.changeStatusFilter,
            type: 'POST',
            data: {
                viewType: TaskListState.currentViewType,
                grouping: TaskListState.currentGrouping,
                statusFilter: statusFilter,
                currentFilters: TaskListState.currentFilters,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                console.log('✅ Status filter changed:', response);

                if (response.status === 'update-view' && response.viewList && response.viewList.length > 0) {
                    response.viewList.forEach(function (item) {
                        if (item.elementId && item.view && item.view.result) {
                            $('#' + item.elementId).html(item.view.result);

                            setTimeout(function () {
                                GroupCollapseManager.applyState();
                                initializeGroupToggle();
                                // ⭐⭐⭐ Initialize tooltips after content load
                                initializeTooltips(document.getElementById(item.elementId));
                            }, 100);

                            if (typeof ModalUtils !== 'undefined') {
                                ModalUtils.processUrlsInContainer($('#' + item.elementId));
                            }
                        }
                    });

                    // ⭐⭐⭐ بروزرسانی state
                    TaskListState.update({
                        currentStatusFilter: statusFilter,
                        currentViewType: response.currentViewType || TaskListState.currentViewType,
                        currentGrouping: response.currentGrouping || TaskListState.currentGrouping
                    });

                    // ⭐⭐⭐ بروزرسانی UI - حذف همه border ها
                    $('.quick-status-filter .p-3').removeClass('border border-2 border-primary border-success border-danger border-warning');

                    // ⭐⭐⭐ اضافه کردن border به دکمه فعلی
                    $this.find('.p-3').addClass('border border-2');

                    if (statusFilter === 1) {
                        $this.find('.p-3').addClass('border-primary');
                    } else if (statusFilter === 2) {
                        $this.find('.p-3').addClass('border-success');
                    } else if (statusFilter === 3) {
                        $this.find('.p-3').addClass('border-danger');
                    } else if (statusFilter === 4) {
                        $this.find('.p-3').addClass('border-warning');
                    }

                    // ⭐⭐⭐ بروزرسانی آمار
                    if (response.stats) {
                        TaskListManager.updateStats(response.stats);
                    }

                    console.log('✅ Filter applied successfully');
                } else {
                    console.error('❌ Invalid response:', response);
                    $('#task-groups-container').html('<div class="alert alert-danger">خطا در دریافت اطلاعات</div>');
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ Error changing status filter:', error);
                console.error('Response:', xhr.responseText);
                $('#task-groups-container').html('<div class="alert alert-danger">خطا در بارگذاری اطلاعات</div>');
            }
        });
    });

    console.log('✅ TaskList initialized successfully');
});