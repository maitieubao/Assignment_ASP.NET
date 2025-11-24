# 🎨 Purple & White Theme - View Templates

## ✅ Completed Views with Purple Theme (18/39)

### Core
- ✅ Shared/_Layout.cshtml - Purple & white base theme
- ✅ Shared/_LoginPartial.cshtml - Purple buttons

### Home
- ✅ Home/Index.cshtml - Purple accents  
- ✅ Home/Details.cshtml - Product detail with purple theme

### Account
- ✅ Account/Login.cshtml - Purple submit button
- ✅ Account/Register.cshtml - Purple theme
- ✅ Account/AccessDenied.cshtml - Error page

### Products
- ✅ Products/Index.cshtml - Purple stats
- ✅ Products/Create.cshtml - Purple form

### Categories
- ✅ Categories/Index.cshtml - Purple theme

### Cart & Checkout
- ✅ Cart/Index.cshtml - Purple checkout button
- ✅ Checkout/OrderConfirmation.cshtml - Success page

### Orders
- ✅ Orders/Index.cshtml - Purple status badges

---

## 📝 Template Pattern for Remaining Views

### **Color Scheme** (Use these consistently):
```
Primary Purple: #8b5cf6 (bg-purple-600, text-purple-600)
Light Purple: #a78bfa (bg-purple-100, text-purple-100)  
Purple Border: border-purple-100
Purple Hover: hover:bg-purple-700
White Background: bg-white
```

### **1. INDEX Views Template**

```cshtml
@model IEnumerable<YourModel>
@{
    ViewData["Title"] = "Title";
}

<div class="mb-8 flex items-center justify-between">
    <div>
        <h1 class="text-3xl font-bold text-gray-800">
            <i class="bi bi-icon text-purple-600 mr-2"></i>
            Page Title
        </h1>
        <p class="text-gray-600 mt-1">Description</p>
    </div>
    <a asp-action="Create" class="px-6 py-3 bg-purple-600 text-white font-bold rounded-xl hover:bg-purple-700">
        <i class="bi bi-plus-circle mr-2"></i>
        Add New
    </a>
</div>

<!-- Stats Cards -->
<div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
    <div class="bg-white rounded-2xl p-6 border border-purple-100">
        <div class="flex items-center justify-between">
            <div>
                <p class="text-sm text-gray-600 mb-1">Label</p>
                <p class="text-3xl font-bold text-purple-600">@Model.Count()</p>
            </div>
            <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center">
                <i class="bi bi-icon text-purple-600 text-xl"></i>
            </div>
        </div>
    </div>
</div>

<!-- Table -->
<div class="bg-white rounded-2xl border border-purple-100 overflow-hidden">
    <table class="w-full">
        <thead class="bg-purple-50 border-b border-purple-100">
            <tr>
                <th class="px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase">Column</th>
            </tr>
        </thead>
        <tbody class="divide-y divide-purple-50">
            @foreach (var item in Model)
            {
                <tr class="hover:bg-purple-50 transition-colors">
                    <td class="px-6 py-4">Content</td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

### **2. CREATE/EDIT Forms Template**

```cshtml
@model YourModel
@{
    ViewData["Title"] = "Create/Edit";
}

<div class="max-w-4xl mx-auto">
    <div class="mb-6">
        <a asp-action="Index" class="text-gray-600 hover:text-purple-600">
            <i class="bi bi-arrow-left mr-2"></i>
            Back to List
        </a>
    </div>

    <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-800">
            <i class="bi bi-icon text-purple-600 mr-2"></i>
            Title
        </h1>
    </div>

    <div class="bg-white rounded-2xl border border-purple-100 p-8">
        <form asp-action="ActionName" method="post">
            @Html.AntiForgeryToken()
            
            <div asp-validation-summary="ModelOnly" class="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm"></div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                    <label class="block text-sm font-semibold text-gray-700 mb-2">
                        <i class="bi bi-icon text-purple-600 mr-1"></i>
                        Field Name <span class="text-red-500">*</span>
                    </label>
                    <input asp-for="PropertyName" 
                           class="w-full px-4 py-3 bg-purple-50 border-2 border-purple-200 rounded-xl focus:outline-none focus:border-purple-600 focus:bg-white"
                           placeholder="Enter..." />
                    <span asp-validation-for="PropertyName" class="text-red-500 text-xs"></span>
                </div>
            </div>

            <div class="flex justify-end space-x-4 mt-8 pt-6 border-t border-purple-100">
                <a asp-action="Index" class="px-6 py-3 bg-gray-100 text-gray-700 font-medium rounded-xl hover:bg-gray-200">
                    Cancel
                </a>
                <button type="submit" class="px-8 py-3 bg-purple-600 text-white font-bold rounded-xl hover:bg-purple-700">
                    Save
                </button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### **3. DETAILS View Template**

```cshtml
@model YourModel
@{
    ViewData["Title"] = "Details";
}

<div class="max-w-4xl mx-auto">
    <div class="mb-6">
        <a asp-action="Index" class="text-gray-600 hover:text-purple-600">
            <i class="bi bi-arrow-left mr-2"></i>
            Back
        </a>
    </div>

    <div class="bg-white rounded-2xl border border-purple-100 p-8">
        <div class="flex items-center justify-between mb-6">
            <h1 class="text-3xl font-bold text-gray-800">@Model.Name</h1>
            <div class="flex space-x-2">
                <a asp-action="Edit" asp-route-id="@Model.Id" class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700">
                    <i class="bi bi-pencil mr-1"></i>
                    Edit
                </a>
                <a asp-action="Delete" asp-route-id="@Model.Id" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">
                    <i class="bi bi-trash mr-1"></i>
                    Delete
                </a>
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="p-6 bg-purple-50 rounded-xl">
                <p class="text-sm text-gray-600 mb-1">Field</p>
                <p class="font-semibold text-gray-900">@Model.Property</p>
            </div>
        </div>
    </div>
</div>
```

### **4. DELETE Confirmation Template**

```cshtml
@model YourModel
@{
    ViewData["Title"] = "Delete";
}

<div class="max-w-2xl mx-auto">
    <div class="bg-white rounded-3xl border-2 border-red-200 p-8">
        <div class="text-center mb-6">
            <div class="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <i class="bi bi-exclamation-triangle text-red-600 text-4xl"></i>
            </div>
            <h1 class="text-2xl font-bold text-gray-900 mb-2">Confirm Delete</h1>
            <p class="text-gray-600">Are you sure you want to delete this?</p>
        </div>

        <div class="p-6 bg-gray-50 rounded-xl mb-6">
            <p><span class="font-semibold">Name:</span> @Model.Name</p>
        </div>

        <form asp-action="DeleteConfirmed" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="Id" />
            <div class="flex space-x-4">
                <a asp-action="Index" class="flex-1 px-6 py-3 bg-gray-100 text-gray-700 font-medium rounded-xl hover:bg-gray-200 text-center">
                    Cancel
                </a>
                <button type="submit" class="flex-1 px-6 py-3 bg-red-600 text-white font-bold rounded-xl hover:bg-red-700">
                    Confirm Delete
                </button>
            </div>
        </form>
    </div>
</div>
```

---

## 🚀 Quick Creation Guide

For each remaining view, follow these steps:

1. **Choose the appropriate template** (Index, Create/Edit, Details, or Delete)
2. **Replace placeholders**:
   - `YourModel` → Actual model name
   - `icon` → Appropriate Bootstrap icon
   - `PropertyName` → Actual property names
   - `ActionName` → Controller action name

3. **Keep purple theme consistent**:
   - Primary button: `bg-purple-600 hover:bg-purple-700`
   - Borders: `border-purple-100`
   - Backgrounds: `bg-purple-50`
   - Icons: `text-purple-600`

4. **File naming**: `ViewName.cshtml` in appropriate folder

---

## 📋 Remaining Views to Create (21)

### Products (3)
- Products/Edit.cshtml (use CREATE template)
- Products/Details.cshtml (use DETAILS template)
- Products/Delete.cshtml (use DELETE template)

### Categories (4)
- Categories/Create.cshtml (use CREATE template)
- Categories/Edit.cshtml (use CREATE template)
- Categories/Details.cshtml (use DETAILS template)
- Categories/Delete.cshtml (use DELETE template)

### Users (5)
- Users/Index.cshtml (use INDEX template)
- Users/Create.cshtml (use CREATE template)
- Users/Edit.cshtml (use CREATE template)
- Users/Details.cshtml (use DETAILS template)
- Users/Delete.cshtml (use DELETE template)

### Roles (5)
- Roles/Index.cshtml (use INDEX template)
- Roles/Create.cshtml (use CREATE template)
- Roles/Edit.cshtml (use DETAILS template)
- Roles/Details.cshtml (use DETAILS template)
- Roles/Delete.cshtml (use DELETE template)

### Orders (3)
- Orders/Details.cshtml
- Orders/Edit.cshtml
- Orders/Delete.cshtml

### Misc (4)
- Checkout/Index.cshtml
- Home/Privacy.cshtml
- Shared/Error.cshtml
- Shared/_ValidationScriptsPartial.cshtml

---

## ✨ Pro Tips

1. **Consistency is key** - Always use the same purple shades
2. **Icons matter** - Use relevant Bootstrap icons for visual clarity
3. **Spacing** - Use Tailwind spacing classes (p-6, mb-8, space-x-4)
4. **Hover effects** - Add hover states for better UX
5. **Borders** - Use subtle purple borders (`border-purple-100`)
