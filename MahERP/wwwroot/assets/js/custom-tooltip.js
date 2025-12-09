/**
 * ========================================
 * ⭐⭐⭐ Custom Tooltip Manager - v2.2 FIXED
 * ========================================
 * Super simple, bulletproof tooltip system
 */

(function() {
    'use strict';
    
    // Tooltip instance storage
    const tooltips = new WeakMap();
    
    class CustomTooltip {
        constructor(element, options = {}) {
            this.element = element;
            this.text = element.getAttribute('data-tooltip') || '';
            this.tooltipPosition = element.getAttribute('data-tooltip-position') || options.position || 'top';
            this.theme = element.getAttribute('data-tooltip-theme') || options.theme || 'dark';
            this.delay = options.delay || 150;
            this.hideDelay = options.hideDelay || 50;
            this.tooltip = null;
            this.showTimer = null;
            this.hideTimer = null;
            
            this.handleMouseEnter = this.handleMouseEnter.bind(this);
            this.handleMouseLeave = this.handleMouseLeave.bind(this);
            this.handleFocus = this.handleFocus.bind(this);
            this.handleBlur = this.handleBlur.bind(this);
            
            this.attachEvents();
        }
        
        attachEvents() {
            this.element.addEventListener('mouseenter', this.handleMouseEnter);
            this.element.addEventListener('mouseleave', this.handleMouseLeave);
            this.element.addEventListener('focus', this.handleFocus);
            this.element.addEventListener('blur', this.handleBlur);
        }
        
        detachEvents() {
            this.element.removeEventListener('mouseenter', this.handleMouseEnter);
            this.element.removeEventListener('mouseleave', this.handleMouseLeave);
            this.element.removeEventListener('focus', this.handleFocus);
            this.element.removeEventListener('blur', this.handleBlur);
        }
        
        handleMouseEnter() {
            this.show();
        }
        
        handleMouseLeave() {
            this.hide();
        }
        
        handleFocus() {
            this.show();
        }
        
        handleBlur() {
            this.hide();
        }
        
        show() {
            clearTimeout(this.hideTimer);
            
            this.showTimer = setTimeout(() => {
                if (!this.text) return;
                
                this.create();
                this.positionTooltip();
                
                // Double RAF for smooth animation
                requestAnimationFrame(() => {
                    requestAnimationFrame(() => {
                        if (this.tooltip) {
                            this.tooltip.classList.add('show');
                        }
                    });
                });
            }, this.delay);
        }
        
        hide() {
            clearTimeout(this.showTimer);
            
            this.hideTimer = setTimeout(() => {
                if (this.tooltip) {
                    this.tooltip.classList.remove('show');
                    setTimeout(() => this.destroy(), 250);
                }
            }, this.hideDelay);
        }
        
        create() {
            this.destroy(); // Remove existing
            
            this.tooltip = document.createElement('div');
            this.tooltip.className = `custom-tooltip ${this.tooltipPosition} theme-${this.theme}`;
            this.tooltip.textContent = this.text;
            
            document.body.appendChild(this.tooltip);
        }
        
        positionTooltip() {
            if (!this.tooltip) return;
            
            // با position: fixed از getBoundingClientRect استفاده می‌کنیم بدون scroll offset
            const rect = this.element.getBoundingClientRect();
            const tooltipRect = this.tooltip.getBoundingClientRect();
            const offset = 10;
            
            let top = 0;
            let left = 0;
            
            switch (this.tooltipPosition) {
                case 'top':
                    top = rect.top - tooltipRect.height - offset;
                    left = rect.left + (rect.width / 2);
                    break;
                    
                case 'bottom':
                    top = rect.bottom + offset;
                    left = rect.left + (rect.width / 2);
                    break;
                    
                case 'left':
                    top = rect.top + (rect.height / 2);
                    left = rect.left - tooltipRect.width - offset;
                    break;
                    
                case 'right':
                    top = rect.top + (rect.height / 2);
                    left = rect.right + offset;
                    break;
            }
            
            // Boundary check
            const margin = 10;
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            
            // Horizontal boundary check for top/bottom positions
            if (this.tooltipPosition === 'top' || this.tooltipPosition === 'bottom') {
                if (left - tooltipRect.width / 2 < margin) {
                    left = margin + tooltipRect.width / 2;
                } else if (left + tooltipRect.width / 2 > viewportWidth - margin) {
                    left = viewportWidth - margin - tooltipRect.width / 2;
                }
            }
            
            // Vertical boundary check for left/right positions
            if (this.tooltipPosition === 'left' || this.tooltipPosition === 'right') {
                if (top - tooltipRect.height / 2 < margin) {
                    top = margin + tooltipRect.height / 2;
                } else if (top + tooltipRect.height / 2 > viewportHeight - margin) {
                    top = viewportHeight - margin - tooltipRect.height / 2;
                }
            }
            
            // Auto-flip if no space
            if (this.tooltipPosition === 'top' && top < margin) {
                // Flip to bottom
                top = rect.bottom + offset;
                this.tooltip.classList.remove('top');
                this.tooltip.classList.add('bottom');
            }
            
            if (this.tooltipPosition === 'bottom' && top + tooltipRect.height > viewportHeight - margin) {
                // Flip to top
                top = rect.top - tooltipRect.height - offset;
                this.tooltip.classList.remove('bottom');
                this.tooltip.classList.add('top');
            }
            
            this.tooltip.style.top = `${top}px`;
            this.tooltip.style.left = `${left}px`;
        }
        
        destroy() {
            if (this.tooltip && this.tooltip.parentNode) {
                this.tooltip.parentNode.removeChild(this.tooltip);
                this.tooltip = null;
            }
        }
        
        cleanup() {
            clearTimeout(this.showTimer);
            clearTimeout(this.hideTimer);
            this.destroy();
            this.detachEvents();
        }
    }
    
    // Static methods
    CustomTooltip.init = function(selector = '[data-tooltip]', options = {}) {
        const elements = document.querySelectorAll(selector);
        let count = 0;
        
        elements.forEach(element => {
            // Skip if already initialized
            if (tooltips.has(element)) {
                return;
            }
            
            const text = element.getAttribute('data-tooltip');
            if (!text) return;
            
            const instance = new CustomTooltip(element, options);
            tooltips.set(element, instance);
            count++;
        });
        
        if (count > 0) {
            console.log(`✅ ${count} custom tooltips initialized`);
        }
        
        return count;
    };
    
    CustomTooltip.destroyAll = function() {
        document.querySelectorAll('[data-tooltip]').forEach(element => {
            const instance = tooltips.get(element);
            if (instance) {
                instance.cleanup();
                tooltips.delete(element);
            }
        });
    };
    
    // Export to window
    window.CustomTooltip = CustomTooltip;
    
    // Auto-initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            CustomTooltip.init();
        });
    } else {
        // DOM already loaded
        CustomTooltip.init();
    }
    
    console.log('✅ CustomTooltip v2.3 loaded successfully');
    
})();
