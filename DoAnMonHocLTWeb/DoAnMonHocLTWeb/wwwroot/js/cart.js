$(document).ready(function () {
    // Add to cart button handler
    $(document).on('click', '.btn-add-cart', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $btn = $(this);
        var productId = $btn.data('id');
        var stock = $btn.data('stock');

        console.log("Add to cart clicked - Product ID:", productId, "Stock:", stock);

        if (!productId) {
            console.error('No product ID found');
            alert('Không tìm thấy ID sản phẩm');
            return;
        }

        if (stock !== undefined && stock <= 0) {
            alert('Sản phẩm đã hết hàng!');
            return;
        }

        var originalText = $btn.html();
        $btn.html('<i class="fas fa-spinner fa-spin"></i>');
        $btn.prop('disabled', true);

        // SỬA URL: /cart/add -> /ShoppingCart/AddToCart
        $.ajax({
            url: '/ShoppingCart/AddToCart',  // ← ĐỔI THÀNH URL NÀY
            type: 'POST',
            data: { productId: productId, quantity: 1 },
            success: function (response) {
                console.log("AJAX Success:", response);
                if (response.success) {
                    $('#cartCount').text(response.cartCount);
                    $('#cartCount').show();
                    $btn.html('<i class="fas fa-check"></i> Added!');
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
                console.error('Status:', status);
                console.error('Response:', xhr.responseText);
                $btn.html(originalText);
                $btn.prop('disabled', false);
                alert('Có lỗi xảy ra, vui lòng thử lại!');
            }
        });
    });
});