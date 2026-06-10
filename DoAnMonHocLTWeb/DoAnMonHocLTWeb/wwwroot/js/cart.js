// cart.js - Xử lý add to cart toàn cục
$(document).ready(function () {
    // Add to cart button handler
    $(document).on('click', '.btn-add-cart, .add-to-cart-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $btn = $(this);
        var productId = $btn.data('id') || $btn.data('product-id');
        var stock = $btn.data('stock');

        if (!productId) {
            console.error('No product ID found');
            return;
        }

        if (stock !== undefined && stock <= 0) {
            alert('Sản phẩm đã hết hàng!');
            return;
        }

        var originalText = $btn.html();
        $btn.html('<i class="fas fa-spinner fa-spin"></i>');
        $btn.prop('disabled', true);

        $.ajax({
            url: '/ShoppingCart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: 1 },
            success: function (response) {
                if (response.success) {
                    $('#cartCount').text(response.cartCount);
                    $('#cartCount').show();
                    $btn.html('<i class="fas fa-check"></i>');
                    setTimeout(function () {
                        $btn.html(originalText);
                        $btn.prop('disabled', false);
                    }, 1500);
                } else {
                    $btn.html(originalText);
                    $btn.prop('disabled', false);
                    alert(response.message || 'Có lỗi xảy ra!');
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', error);
                $btn.html(originalText);
                $btn.prop('disabled', false);
                alert('Có lỗi xảy ra, vui lòng thử lại!');
            }
        });
    });
});