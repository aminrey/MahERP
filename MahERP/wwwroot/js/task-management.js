/**
 * Task Management Scripts
 */
 
// Task operations handling
class TaskOperations {
    static init() {
        // Initialize event listeners
        this.initToggleOperationStatus();
        this.initTaskApprovals();
    }
    
    static initToggleOperationStatus() {
        // Toggle operation completion status
        $(document).on('change', '.js-task-status input[type="checkbox"]', function() {
            const operationId = $(this).closest('.js-task').data('task-id');
            const isCompleted = $(this).prop('checked');
            
            // Send AJAX request to update operation status
            $.ajax({
                url: '/AdminArea/Task/ToggleOperationCompletion',
                type: 'POST',
                data: { id: operationId },
                success: function(result) {
                    if (result.success) {
                        // Visual feedback for success
                        Dashmix.helpers('jq-notify', {
                            type: 'success',
                            icon: 'fa fa-check me-1',
                            message: isCompleted ? '?????? ?? ?????? ????? ??' : '?????? ?? ???? ?? ??? ????? ?????'
                        });
                        
                        // Update UI
                        const $task = $(`.js-task[data-task-id="${operationId}"]`);
                        const $content = $task.find('.js-task-content');
                        
                        if (isCompleted) {
                            $task.attr('data-task-completed', 'true');
                            $content.wrapInner('<del></del>');
                        } else {
                            $task.attr('data-task-completed', 'false');
                            $content.find('del').contents().unwrap();
                        }
                        
                        // Update task progress if needed
                        updateTaskProgress();
                    } else {
                        // Error notification
                        Dashmix.helpers('jq-notify', {
                            type: 'danger',
                            icon: 'fa fa-times me-1',
                            message: '???: ' + (result.message || '?????? ?? ??? ????? ??')
                        });
                        
                        // Revert checkbox state
                        $(this).prop('checked', !isCompleted);
                    }
                }.bind(this),
                error: function() {
                    // Error notification
                    Dashmix.helpers('jq-notify', {
                        type: 'danger',
                        icon: 'fa fa-times me-1',
                        message: '??? ?? ?????? ?? ????'
                    });
                    
                    // Revert checkbox state
                    $(this).prop('checked', !isCompleted);
                }.bind(this)
            });
        });
    }
    
    static initTaskApprovals() {
        // Supervisor approval
        $(document).on('click', '.js-approve-supervisor', function() {
            const taskId = $(this).data('task-id');
            
            $.ajax({
                url: '/AdminArea/Task/ApproveTaskBySupervisor',
                type: 'POST',
                data: { id: taskId },
                success: function(result) {
                    if (result.success) {
                        // Refresh the page to show updated state
                        location.reload();
                    } else {
                        Dashmix.helpers('jq-notify', {
                            type: 'danger',
                            icon: 'fa fa-times me-1',
                            message: '???: ' + (result.message || '?????? ?? ??? ????? ??')
                        });
                    }
                },
                error: function() {
                    Dashmix.helpers('jq-notify', {
                        type: 'danger',
                        icon: 'fa fa-times me-1',
                        message: '??? ?? ?????? ?? ????'
                    });
                }
            });
        });
        
        // Manager approval
        $(document).on('click', '.js-approve-manager', function() {
            const taskId = $(this).data('task-id');
            
            $.ajax({
                url: '/AdminArea/Task/ApproveTaskByManager',
                type: 'POST',
                data: { id: taskId },
                success: function(result) {
                    if (result.success) {
                        // Refresh the page to show updated state
                        location.reload();
                    } else {
                        Dashmix.helpers('jq-notify', {
                            type: 'danger',
                            icon: 'fa fa-times me-1',
                            message: '???: ' + (result.message || '?????? ?? ??? ????? ??')
                        });
                    }
                },
                error: function() {
                    Dashmix.helpers('jq-notify', {
                        type: 'danger',
                        icon: 'fa fa-times me-1',
                        message: '??? ?? ?????? ?? ????'
                    });
                }
            });
        });
    }
    
    // Calculate and update task progress
    static updateTaskProgress() {
        const $taskList = $('.js-task-list');
        const totalTasks = $taskList.find('.js-task').length;
        const completedTasks = $taskList.find('.js-task[data-task-completed="true"]').length;
        
        if (totalTasks > 0) {
            const progressPercentage = Math.round((completedTasks / totalTasks) * 100);
            $('.js-task-progress-bar').css('width', progressPercentage + '%').text(progressPercentage + '%');
            
            // Update progress badge
            $('.js-task-progress-badge').text(completedTasks + '/' + totalTasks);
        }
    }
}

// Initialize on document ready
$(function() {
    // Initialize task operations
    TaskOperations.init();
});