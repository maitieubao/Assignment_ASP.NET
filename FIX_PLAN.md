# 🔧 COMPREHENSIVE FIX PLAN - 6 TASKS

## ✅ TASK 1: Backend Logic Fixes

### 1.1 Remove Promo Code UI (No backend logic)
- Remove promo code section from Cart/Index.cshtml

### 1.2 Fix Cart Count Display
- ✅ Created CartCountViewComponent.cs
- ✅ Updated Layout to use @await Component.InvokeAsync("CartCount")

## ✅ TASK 2: Remove Transform/Scale Effects

Files to update:
- Shared/_Layout.cshtml - ✅ Removed translateY
- Home/Index.cshtml - Remove scale-105
- Products/Index.cshtml - Remove hover:scale-105
- Products/Create.cshtml - Remove hover:scale-105  
- Cart/Index.cshtml - Remove hover:scale-105
- Checkout/OrderConfirmation.cshtml - Remove all animations
- Account/AccessDenied.cshtml - Remove hover:scale-105

## 🚀 TASK 3: Purchase History for Customers

Create new views:
- Account/OrderHistory.cshtml - List all orders for logged-in customer
- Update AccountController with OrderHistory action
- Add link in navigation/profile dropdown

## 🚀 TASK 4: User Profile/Info for All Roles

Create:
- Account/Profile.cshtml - View/edit user information
- Account/EditProfile.cshtml - Edit form
- AccountController actions: Profile(), EditProfile(POST)
- Add "Thông tin" link in LoginPartial dropdown

## 🔴 TASK 5: Fix Categories Management Error

Need to investigate:
- Check CategoriesController for errors
- Check Categories views for issues
- Common issue: Missing ViewBag data, null reference errors

## 🔍 TASK 6: Scan & Fix All UI Errors

Check:
- Missing Bootstrap Icons CDN link in Layout
- Null reference warnings in views
- Missing validation
- Broken links
- Responsive design issues

## EXECUTION ORDER:
1. ✅ TASK 2 (Partially done - need to finish)
2. TASK 6 (Fix UI errors first)
3. TASK 5 (Fix Categories)
4. TASK 1 (Clean up unused features)
5. TASK 3 (Purchase history)
6. TASK 4 (User profile)

Starting implementation now...
