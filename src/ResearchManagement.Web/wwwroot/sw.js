// ضع هذا الكود في _Layout.cshtml أو في ملف JavaScript منفصل

// تحقق من دعم Service Worker قبل التسجيل
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/sw.js', {
            scope: '/' // تحديد النطاق بوضوح
        })
            .then(function (registration) {
                console.log('✅ Service Worker تم تسجيله بنجاح:', registration.scope);

                // التحقق من التحديثات
                registration.addEventListener('updatefound', function () {
                    console.log('🔄 يتم تحديث Service Worker...');
                });
            })
            .catch(function (error) {
                console.log('❌ فشل تسجيل Service Worker:', error);

                // إذا فشل التسجيل، لا تعطل الموقع
                console.log('🔄 الموقع سيعمل بدون Service Worker');
            });
    });

    // الاستماع لرسائل Service Worker
    navigator.serviceWorker.addEventListener('message', function (event) {
        console.log('📩 رسالة من Service Worker:', event.data);
    });
} else {
    console.log('⚠️ المتصفح لا يدعم Service Workers');
}