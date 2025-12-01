// Category toggle functionality
document.addEventListener('DOMContentLoaded', function() {
    const categoryLinks = document.querySelectorAll('.category-link');
    const categoriesContainer = document.querySelector('.space-y-1');
    
    if (categoryLinks.length > 0 && categoriesContainer) {
        // Make categories display in grid
        categoriesContainer.classList.remove('space-y-1');
        categoriesContainer.classList.add('grid', 'grid-cols-2', 'gap-2');
        
        // Get all links in the container (including "All products")
        const allLinks = categoriesContainer.querySelectorAll('a');
        
        // Update styles for all links
        allLinks.forEach((link, index) => {
            link.classList.remove('block', 'px-4', 'py-2.5', 'rounded-lg');
            link.classList.add('px-3', 'py-2', 'text-center', 'text-xs', 'rounded-lg');
            
            // Remove icons to save space
            const icon = link.querySelector('i');
            if (icon) {
                icon.remove();
            }
            
            // Hide categories after the 7th one (index 6, because first is "All products")
            if (index > 6) {
                link.classList.add('category-hidden', 'hidden');
            }
        });
        
        // Add toggle button if there are more than 7 items
        if (allLinks.length > 7) {
            const toggleBtn = document.createElement('button');
            toggleBtn.className = 'col-span-2 text-indigo-600 hover:text-indigo-700 text-xs font-medium mt-2 w-full text-center py-2 rounded-lg hover:bg-indigo-50 transition-colors';
            toggleBtn.innerHTML = '<i class="bi bi-chevron-down mr-1"></i>Xem thêm';
            toggleBtn.onclick = function() {
                const hiddenCategories = document.querySelectorAll('.category-hidden');
                const icon = this.querySelector('i');
                
                hiddenCategories.forEach(cat => {
                    cat.classList.toggle('hidden');
                });
                
                if (icon.classList.contains('bi-chevron-down')) {
                    icon.classList.remove('bi-chevron-down');
                    icon.classList.add('bi-chevron-up');
                    this.innerHTML = '<i class="bi bi-chevron-up mr-1"></i>Thu gọn';
                } else {
                    icon.classList.remove('bi-chevron-up');
                    icon.classList.add('bi-chevron-down');
                    this.innerHTML = '<i class="bi bi-chevron-down mr-1"></i>Xem thêm';
                }
            };
            
            categoriesContainer.parentElement.appendChild(toggleBtn);
        }
    }
});
