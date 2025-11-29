using AutoMapper;
using MahERP.CommonLayer.PublicClasses;
using MahERP.DataModelLayer.Entities.TaskManagement;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels;
using MahERP.DataModelLayer.ViewModels.taskingModualsViewModels.TaskViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MahERP.DataModelLayer.Repository.Tasking
{
    /// <summary>
    /// اعتبارسنجی و ساخت Entity تسک
    /// </summary>
    public partial class TaskRepository
    {
        #region Task Creation and Validation Helper Methods

        /// <summary>
        /// اعتبارسنجی مدل تسک قبل از ایجاد یا ویرایش
        /// </summary>
        public async Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateTaskModelAsync(TaskViewModel model, string userId)
        {
            var errors = new Dictionary<string, string>();
            var isValid = true;

            try
            {
                // بررسی شعبه
                if (model.BranchIdSelected <= 0)
                {
                    errors.Add("BranchIdSelected", "انتخاب شعبه الزامی است");
                    isValid = false;
                }
                else
                {
                    var userBranches = _BranchRipository.GetBrnachListByUserId(userId);
                    if (!userBranches.Any(b => b.Id == model.BranchIdSelected))
                    {
                        errors.Add("BranchIdSelected", "شما به شعبه انتخاب شده دسترسی ندارید");
                        isValid = false;
                    }

                    // ⭐⭐⭐ بررسی تعلق Contact به Branch
                    if (model.SelectedContactId.HasValue && model.SelectedContactId.Value > 0)
                    {
                        if (!_BranchRipository.IsContactAssignedToBranch(model.BranchIdSelected, model.SelectedContactId.Value))
                        {
                            errors.Add("SelectedContactId", "فرد انتخابی به شعبه انتخاب شده تعلق ندارد");
                            isValid = false;
                        }
                    }

                    // ⭐⭐⭐ بررسی تعلق Organization به Branch
                    if (model.SelectedOrganizationId.HasValue && model.SelectedOrganizationId.Value > 0)
                    {
                        if (!_BranchRipository.IsOrganizationAssignedToBranch(model.BranchIdSelected, model.SelectedOrganizationId.Value))
                        {
                            errors.Add("SelectedOrganizationId", "سازمان انتخابی به شعبه انتخاب شده تعلق ندارد");
                            isValid = false;
                        }
                    }
                }

                // بررسی کد دستی
                if (model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode))
                {
                    if (!_taskCodeGenerator.ValidateTaskCode(model.ManualTaskCode))
                    {
                        var settings = _taskCodeGenerator.GetTaskCodeSettings();
                        errors.Add("ManualTaskCode",
                            $"کد تسک نامعتبر است. نمی‌بایست از پیشوند '{settings.SystemPrefix}-' استفاده کنید");
                        isValid = false;
                    }
                }

                return (isValid, errors);
            }
            catch (Exception ex)
            {
                errors.Add("General", $"خطا در اعتبارسنجی: {ex.Message}");
                return (false, errors);
            }
        }

        /// <summary>
        /// ایجاد entity تسک از ViewModel - بروزرسانی شده
        /// </summary>
        public async Task<Tasks> CreateTaskEntityAsync(TaskViewModel model, string currentUserId, IMapper mapper)
        {
            try
            {
                string finalTaskCode = model.IsManualTaskCode && !string.IsNullOrWhiteSpace(model.ManualTaskCode)
                    ? model.ManualTaskCode
                    : _taskCodeGenerator.GenerateTaskCode();

                var task = mapper.Map<Tasks>(model);
                task.TaskCode = finalTaskCode;
                task.CreateDate = DateTime.Now;
                task.CreatorUserId = currentUserId;
                task.IsActive = model.IsActive;
                task.IsDeleted = false;
                task.TaskTypeInput = 1;

                // ⭐⭐⭐ مقدار پیش‌فرض تیمی
                task.VisibilityLevel = model.VisibilityLevel > 0 ? model.VisibilityLevel : (byte)2;

                task.Priority = 0;
                task.Important = false;
                task.Status = 0;
                task.CreationMode = 0;
                task.TaskCategoryId = model.TaskCategoryIdSelected;
                task.BranchId = model.BranchIdSelected;

                // ⭐⭐⭐ OLD - Stakeholder (حفظ برای backward compatibility)
                task.StakeholderId = model.StakeholderId;

                // ⭐⭐⭐ NEW - Contact & Organization
                task.ContactId = model.SelectedContactId;
                task.OrganizationId = model.SelectedOrganizationId;
                if (!task.OrganizationId.HasValue && task.ContactId.HasValue)
                {
                    // پیدا کردن اولین سازمان مرتبط با Contact
                    var contactOrganization = await _context.OrganizationContact_Tbl
                        .Where(oc => oc.ContactId == task.ContactId.Value && oc.IsActive && oc.IsPrimary)
                        .FirstOrDefaultAsync();

                    if (contactOrganization != null)
                    {
                        task.OrganizationId = contactOrganization.OrganizationId;
                    }
                    else
                    {
                        // اگر سازمان اصلی نداشت، اولین سازمان را انتخاب کن
                        var anyContactOrganization = await _context.OrganizationContact_Tbl
                            .Where(oc => oc.ContactId == task.ContactId.Value && oc.IsActive)
                            .FirstOrDefaultAsync();

                        if (anyContactOrganization != null)
                        {
                            task.OrganizationId = anyContactOrganization.OrganizationId;
                        }
                    }
                }

                // تبدیل تاریخ‌های شمسی
                if (!string.IsNullOrEmpty(model.SuggestedStartDatePersian))
                {
                    task.DueDate = ConvertDateTime.ConvertShamsiToMiladi(model.SuggestedStartDatePersian);
                }

                if (!string.IsNullOrEmpty(model.StartDatePersian))
                {
                    task.StartDate = ConvertDateTime.ConvertShamsiToMiladi(model.StartDatePersian);
                }

                // ذخیره در دیتابیس
                _unitOfWork.TaskUW.Create(task);
                await _unitOfWork.SaveAsync();

                return task;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ایجاد entity تسک: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ذخیره عملیات‌ها و یادآوری‌های تسک
        /// </summary>
        public async Task SaveTaskOperationsAndRemindersAsync(int taskId, TaskViewModel model)
        {
            try
            {
                // ذخیره عملیات‌ها
                if (!string.IsNullOrEmpty(model.TaskOperationsJson))
                {
                    try
                    {
                        var operations = System.Text.Json.JsonSerializer.Deserialize<List<TaskOperationViewModel>>(model.TaskOperationsJson);
                        if (operations?.Any() == true)
                        {
                            SaveTaskOperations(taskId, operations);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ذخیره عملیات‌ها: {ex.Message}");
                    }
                }

                // ذخیره یادآوری‌ها
                if (!string.IsNullOrEmpty(model.TaskRemindersJson))
                {
                    try
                    {
                        var reminders = System.Text.Json.JsonSerializer.Deserialize<List<TaskReminderViewModel>>(model.TaskRemindersJson);
                        if (reminders?.Any() == true)
                        {
                            SaveTaskReminders(taskId, reminders);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"خطا در ذخیره یادآوری‌ها: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در ذخیره عملیات‌ها و یادآوری‌ها: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ذخیره فایل‌های پیوست تسک
        /// </summary>
        public async Task SaveTaskAttachmentsAsync(int taskId, List<IFormFile> files, string uploaderUserId, string webRootPath)
        {
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("⚠️ No files to save");
                return;
            }

            // ⭐ ایجاد پوشه uploads
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "tasks", taskId.ToString());

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                Console.WriteLine($"📁 Created folder: {uploadsFolder}");
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        // ⭐ نام فایل یکتا
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // ⭐ ذخیره فایل
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // ⭐ ذخیره رکورد در دیتابیس
                        var attachment = new TaskAttachment
                        {
                            TaskId = taskId,
                            FileName = file.FileName,
                            FilePath = $"/uploads/tasks/{taskId}/{fileName}",
                            FileSize = file.Length,
                            UploaderUserId = uploaderUserId,
                            UploadDate = DateTime.Now
                        };

                        _context.TaskAttachment_Tbl.Add(attachment);
                        Console.WriteLine($"✅ File saved: {file.FileName} ({file.Length} bytes)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error saving file {file.FileName}: {ex.Message}");
                        throw;
                    }
                }
            }

            _context.SaveChanges();
        }

        #endregion
    }
}
