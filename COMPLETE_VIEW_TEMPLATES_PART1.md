# 🎨 Complete View Templates - White Theme
# Copy-paste these into respective files

## =================================
## PRODUCTS VIEWS
## =================================

### Products/Edit.cshtml
```cshtml
@model Assignment_ASP.NET.Models.Product
@{
    ViewData["Title"] = "Sửa sản phẩm";
}

<div class="max-w-4xl mx-auto">
    <div class="mb-6">
        <a asp-action="Index" class="text-slate-600 hover:text-indigo-600 text-sm">
            <i class="bi bi-arrow-left mr-2"></i>
            Quay lại
        </a>
    </div>

    <div class="mb-8">
        <h1 class="text-2xl font-bold text-slate-900">
            <i class="bi bi-pencil text-indigo-600 mr-2"></i>
            Sửa sản phẩm
        </h1>
    </div>

    <div class="bg-white rounded-2xl border border-slate-200 p-8">
        <form asp-action="Edit" method="post" enctype="multipart/form-data">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="ProductID" />
            
            <div asp-validation-summary="ModelOnly" class="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm"></div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                
                <div class="md:col-span-2">
                    <label asp-for="ProductName" class="block text-sm font-semibold text-slate-700 mb-2">
                        Tên sản phẩm <span class="text-red-500">*</span>
                    </label>
                    <input asp-for="ProductName" 
                           class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-indigo-500 focus:bg-white text-sm"/>
                    <span asp-validation-for="ProductName" class="text-red-500 text-xs"></span>
                </div>

                <div>
                    <label asp-for="Price" class="block text-sm font-semibold text-slate-700 mb-2">
                        Giá <span class="text-red-500">*</span>
                    </label>
                    <input asp-for="Price" type="number"
                           class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-indigo-500 focus:bg-white text-sm"/>
                    <span asp-validation-for="Price" class="text-red-500 text-xs"></span>
                </div>

                <div>
                    <label asp-for="StockQuantity" class="block text-sm font-semibold text-slate-700 mb-2">
                        Số lượng <span class="text-red-500">*</span>
                    </label>
                    <input asp-for="StockQuantity" type="number"
                           class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-indigo-500 focus:bg-white text-sm"/>
                    <span asp-validation-for="StockQuantity" class="text-red-500 text-xs"></span>
                </div>

                <div>
                    <label asp-for="CategoryID" class="block text-sm font-semibold text-slate-700 mb-2">
                        Danh mục <span class="text-red-500">*</span>
                    </label>
                    <select asp-for="CategoryID" asp-items="ViewBag.Categories"
                            class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-indigo-500 text-sm">
                        <option value="">-- Chọn --</option>
                    </select>
                    <span asp-validation-for="CategoryID" class="text-red-500 text-xs"></span>
                </div>

                <div>
                    <label class="block text-sm font-semibold text-slate-700 mb-2">
                        Hình ảnh mới
                    </label>
                    <input type="file" name="imageFile" accept="image/*"
                           class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg text-sm file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-indigo-50 file:text-indigo-700"/>
                </div>

                <div class="md:col-span-2">
                    <label asp-for="Description" class="block text-sm font-semibold text-slate-700 mb-2">
                        Mô tả
                    </label>
                    <textarea asp-for="Description" rows="4"
                              class="w-full px-4 py-3 bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:border-indigo-500 focus:bg-white resize-none text-sm"></textarea>
                </div>
            </div>

            <div class="flex justify-end space-x-4 mt-8 pt-6 border-t border-slate-200">
                <a asp-action="Index" class="px-6 py-3 bg-slate-100 text-slate-700 font-medium rounded-lg hover:bg-slate-200 text-sm">
                    Hủy
                </a>
                <button type="submit" class="px-8 py-3 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 text-sm">
                    Lưu
                </button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### Products/Details.cshtml
```cshtml
@model Assignment_ASP.NET.Models.Product
@{
    ViewData["Title"] = "Chi tiết sản phẩm";
}

<div class="max-w-4xl mx-auto">
    <div class="mb-6">
        <a asp-action="Index" class="text-slate-600 hover:text-indigo-600 text-sm">
            <i class="bi bi-arrow-left mr-2"></i>
            Quay lại
        </a>
    </div>

    <div class="bg-white rounded-2xl border border-slate-200 p-8">
        <div class="flex items-center justify-between mb-6">
            <h1 class="text-2xl font-bold text-slate-900">@Model.ProductName</h1>
            <div class="flex space-x-2">
                <a asp-action="Edit" asp-route-id="@Model.ProductID" class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 text-sm font-medium">
                    <i class="bi bi-pencil mr-1"></i>
                    Sửa
                </a>
                <a asp-action="Delete" asp-route-id="@Model.ProductID" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 text-sm font-medium">
                    <i class="bi bi-trash mr-1"></i>
                    Xóa
                </a>
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p class="text-xs text-slate-600 mb-1">Giá</p>
                <p class="font-bold text-slate-900 text-xl">@Model.Price.ToString("N0")₫</p>
            </div>
            <div class="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p class="text-xs text-slate-600 mb-1">Số lượng kho</p>
                <p class="font-bold text-slate-900 text-xl">@Model.StockQuantity</p>
            </div>
            <div class="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p class="text-xs text-slate-600 mb-1">Danh mục</p>
                <p class="font-semibold text-slate-900">@Model.Category?.CategoryName</p>
            </div>
            <div class="p-5 bg-slate-50 rounded-xl border border-slate-200">
                <p class="text-xs text-slate-600 mb-1">Mô tả</p>
                <p class="text-slate-700 text-sm">@(Model.Description ?? "Không có")</p>
            </div>
        </div>
    </div>
</div>
```

### Products/Delete.cshtml
```cshtml
@model Assignment_ASP.NET.Models.Product
@{
    ViewData["Title"] = "Xóa sản phẩm";
}

<div class="max-w-2xl mx-auto">
    <div class="bg-white rounded-2xl border-2 border-red-200 p-8">
        <div class="text-center mb-6">
            <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <i class="bi bi-exclamation-triangle text-red-600 text-3xl"></i>
            </div>
            <h1 class="text-xl font-bold text-slate-900 mb-2">Xác nhận xóa</h1>
            <p class="text-slate-600 text-sm">Bạn có chắc muốn xóa sản phẩm này?</p>
        </div>

        <div class="p-5 bg-slate-50 rounded-xl border border-slate-200 mb-6">
            <p class="text-sm"><span class="font-semibold">Tên:</span> @Model.ProductName</p>
            <p class="text-sm mt-2"><span class="font-semibold">Giá:</span> @Model.Price.ToString("N0")₫</p>
        </div>

        <form asp-action="DeleteConfirmed" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="ProductID" />
            <div class="flex space-x-4">
                <a asp-action="Index" class="flex-1 px-6 py-3 bg-slate-100 text-slate-700 font-medium rounded-lg hover:bg-slate-200 text-center text-sm">
                    Hủy
                </a>
                <button type="submit" class="flex-1 px-6 py-3 bg-red-600 text-white font-semibold rounded-lg hover:bg-red-700 text-sm">
                    Xác nhận xóa
                </button>
            </div>
        </form>
    </div>
</div>
```

---

## Copy tất cả code trên vào các file tương ứng!
## Tiếp tục với Categories, Users, Roles, Orders trong message tiếp theo...
