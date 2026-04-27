<template>
  <div class="landing-page">
    <!-- Hero Section -->
    <header class="hero">
      <nav class="navbar">
        <div class="container">
          <div class="logo">
            <span class="logo-icon">🏥</span>
            <span class="logo-text">An Phúc Sài Gòn</span>
          </div>
          <div class="nav-links">
            <a href="#about">Giới thiệu</a>
            <a href="#services">Dịch vụ</a>
            <a href="#contact">Liên hệ</a>
            <router-link to="/login" class="btn-login">Đăng nhập</router-link>
          </div>
        </div>
      </nav>
      
      <div class="hero-content">
        <div class="container">
          <h1>Phòng Khám Đa Khoa<br>An Phúc Sài Gòn</h1>
          <p class="hero-subtitle">
            Chuyên tổ chức khám sức khỏe định kỳ cho doanh nghiệp, công ty, nhà máy
          </p>
          <div class="hero-actions">
            <a href="#contact" class="btn-primary">Đăng ký khám ngay</a>
            <a href="#services" class="btn-secondary">Tìm hiểu thêm</a>
          </div>
        </div>
      </div>
    </header>

    <!-- Stats Section -->
    <section class="stats">
      <div class="container">
        <div class="stats-grid">
          <div class="stat-item">
            <span class="stat-number">15+</span>
            <span class="stat-label">Năm kinh nghiệm</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">500+</span>
            <span class="stat-label">Doanh nghiệp đối tác</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">100K+</span>
            <span class="stat-label">Lượt khám mỗi năm</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">50+</span>
            <span class="stat-label">Bác sĩ chuyên khoa</span>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section id="about" class="about">
      <div class="container">
        <div class="section-header">
          <h2>Về Chúng Tôi</h2>
          <p>Phòng khám An Phúc Sài Gòn - Đối tác tin cậy trong chăm sóc sức khỏe doanh nghiệp</p>
        </div>
        <div class="about-grid">
          <div class="about-card">
            <span class="icon">🚑</span>
            <h3>Khám Ngoại Tuyến</h3>
            <p>Đội ngũ y bác sĩ trực tiếp đến tận công ty để khám sức khỏe, tiết kiệm thời gian cho doanh nghiệp</p>
          </div>
          <div class="about-card">
            <span class="icon">🏢</span>
            <h3>Trang Thiết Bị Hiện Đại</h3>
            <p>Đầu tư máy móc, thiết bị y tế tiên tiến đảm bảo kết quả khám chính xác</p>
          </div>
          <div class="about-card">
            <span class="icon">👨‍⚕️</span>
            <h3>Đội Ngũ Chuyên Nghiệp</h3>
            <p>Bác sĩ chuyên khoa giàu kinh nghiệm, được đào tạo bài bản tại các bệnh viện lớn</p>
          </div>
          <div class="about-card">
            <span class="icon">📊</span>
            <h3>Báo Cáo Chi Tiết</h3>
            <p>Hệ thống báo cáo sức khỏe điện tử, lưu trữ và quản lý dễ dàng</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Services Section -->
    <section id="services" class="services">
      <div class="container">
        <div class="section-header">
          <h2>Dịch Vụ Khám Sức Khỏe</h2>
          <p>Gói khám đa dạng, phù hợp với mọi nhu cầu của doanh nghiệp</p>
        </div>
        <div class="services-grid">
          <div class="service-card">
            <div class="service-header">
              <h3>Khám Sức Khỏe Cơ Bản</h3>
              <span class="price">Liên hệ</span>
            </div>
            <ul>
              <li>Khám nội tổng quát</li>
              <li>Xét nghiệm máu cơ bản</li>
              <li>Siêu âm bụng</li>
              <li>Điện tâm đồ</li>
              <li>X-quang phổi</li>
            </ul>
          </div>
          <div class="service-card featured">
            <div class="badge">Phổ biến</div>
            <div class="service-header">
              <h3>Khám Sức Khỏe Toàn Diện</h3>
              <span class="price">Liên hệ</span>
            </div>
            <ul>
              <li>Tất cả gói Cơ Bản</li>
              <li>Tầm soát ung thư</li>
              <li>Nội soi dạ dày</li>
              <li>CT/MRI (nếu cần)</li>
              <li>Tư vấn dinh dưỡng</li>
            </ul>
          </div>
          <div class="service-card">
            <div class="service-header">
              <h3>Khám Theo Yêu Cầu</h3>
              <span class="price">Liên hệ</span>
            </div>
            <ul>
              <li>Khám sức khỏe lao động</li>
              <li>Khám thẻ xanh cho F0</li>
              <li>Khám tiền hôn nhân</li>
              <li>Khám định kỳ cá nhân</li>
              <li>Tư vấn gói tùy chỉnh</li>
            </ul>
          </div>
        </div>
      </div>
    </section>

    <!-- Process Section -->
    <section class="process">
      <div class="container">
        <div class="section-header">
          <h2>Quy Trình Đăng Ký</h2>
          <p>Đơn giản, nhanh chóng chỉ với 4 bước</p>
        </div>
        <div class="process-steps">
          <div class="step">
            <span class="step-number">1</span>
            <h4>Liên hệ</h4>
            <p>Gửi thông tin doanh nghiệp qua form hoặc hotline</p>
          </div>
          <div class="step-arrow">→</div>
          <div class="step">
            <span class="step-number">2</span>
            <h4>Tư vấn</h4>
            <p>Nhân viên tư vấn gói khám phù hợp</p>
          </div>
          <div class="step-arrow">→</div>
          <div class="step">
            <span class="step-number">3</span>
            <h4>Ký hợp đồng</h4>
            <p>Thống nhất lịch khám và ký hợp đồng</p>
          </div>
          <div class="step-arrow">→</div>
          <div class="step">
            <span class="step-number">4</span>
            <h4>Khám sức khỏe</h4>
            <p>Đội ngũ y tế đến khám tại công ty</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Contact Section -->
    <section id="contact" class="contact">
      <div class="container">
        <div class="section-header">
          <h2>Đăng Ký Tư Vấn</h2>
          <p>Để lại thông tin, chúng tôi sẽ liên hệ trong 24h</p>
        </div>
        <div class="contact-wrapper">
          <form @submit.prevent="submitForm" class="contact-form">
            <div class="form-group">
              <label>Tên công ty *</label>
              <input v-model="form.companyName" type="text" required placeholder="Công ty TNHH ABC">
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Người liên hệ *</label>
                <input v-model="form.contactName" type="text" required placeholder="Họ và tên">
              </div>
              <div class="form-group">
                <label>Số điện thoại *</label>
                <input v-model="form.phone" type="tel" required placeholder="0901234567">
              </div>
            </div>
            <div class="form-group">
              <label>Email</label>
              <input v-model="form.email" type="email" placeholder="email@company.com">
            </div>
            <div class="form-group">
              <label>Số lượng người khám</label>
              <input v-model="form.quantity" type="number" placeholder="VD: 100">
            </div>
            <div class="form-group">
              <label>Gói khám quan tâm</label>
              <select v-model="form.package">
                <option value="">-- Chọn gói --</option>
                <option value="basic">Khám Cơ Bản</option>
                <option value="comprehensive">Khám Toàn Diện</option>
                <option value="custom">Tùy Chỉnh</option>
              </select>
            </div>
            <div class="form-group">
              <label>Ghi chú</label>
              <textarea v-model="form.note" rows="3" placeholder="Yêu cầu đặc biệt hoặc thời gian mong muốn..."></textarea>
            </div>
            <button type="submit" class="btn-submit" :disabled="submitting">
              {{ submitting ? 'Đang gửi...' : 'Gửi yêu cầu' }}
            </button>
          </form>
          
          <div class="contact-info">
            <h3>Thông tin liên hệ</h3>
            <div class="info-item">
              <span class="icon">📍</span>
              <div>
                <strong>Địa chỉ</strong>
                <p>123 Nguyễn Văn A, P. Bình Thọ, TP. Thủ Đức, TP.HCM</p>
              </div>
            </div>
            <div class="info-item">
              <span class="icon">📞</span>
              <div>
                <strong>Hotline</strong>
                <p>1900 1234 (7h30 - 17h30, T2-T7)</p>
              </div>
            </div>
            <div class="info-item">
              <span class="icon">📧</span>
              <div>
                <strong>Email</strong>
                <p>contact@anphuckham.com</p>
              </div>
            </div>
            <div class="info-item">
              <span class="icon">🌐</span>
              <div>
                <strong>Website</strong>
                <p>www.anphuckham.com</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="container">
        <div class="footer-content">
          <div class="footer-logo">
            <span class="logo-icon">🏥</span>
            <span>An Phúc Sài Gòn</span>
          </div>
          <p class="copyright">© 2026 Phòng Khám Đa Khoa An Phúc Sài Gòn. All rights reserved.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const form = ref({
  companyName: '',
  contactName: '',
  phone: '',
  email: '',
  quantity: '',
  package: '',
  note: ''
})

const submitting = ref(false)

const submitForm = async () => {
  submitting.value = true
  try {
    // Gửi thông tin đến API (cần tạo endpoint riêng nếu muốn lưu DB)
    // Hiện tại chỉ hiển thị alert
    alert('Cảm ơn bạn đã đăng ký! Chúng tôi sẽ liên hệ trong 24h.')
    
    // Reset form
    form.value = {
      companyName: '',
      contactName: '',
      phone: '',
      email: '',
      quantity: '',
      package: '',
      note: ''
    }
  } catch (error) {
    alert('Có lỗi xảy ra, vui lòng thử lại sau.')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.landing-page {
  min-height: 100vh;
  font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

/* Hero */
.hero {
  background: linear-gradient(135deg, #1a5fb4 0%, #1c71d8 100%);
  color: white;
  min-height: 100vh;
}

.navbar {
  padding: 20px 0;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.navbar .container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.5rem;
  font-weight: 700;
}

.logo-icon {
  font-size: 2rem;
}

.nav-links {
  display: flex;
  gap: 30px;
  align-items: center;
}

.nav-links a {
  color: white;
  text-decoration: none;
  font-weight: 500;
  transition: opacity 0.3s;
}

.nav-links a:hover {
  opacity: 0.8;
}

.btn-login {
  background: white;
  color: #1a5fb4 !important;
  padding: 10px 24px;
  border-radius: 8px;
  font-weight: 600;
}

.hero-content {
  padding: 120px 0 180px;
  text-align: center;
}

.hero h1 {
  font-size: 3.5rem;
  font-weight: 800;
  margin-bottom: 20px;
  line-height: 1.2;
}

.hero-subtitle {
  font-size: 1.25rem;
  opacity: 0.9;
  max-width: 600px;
  margin: 0 auto 40px;
  line-height: 1.6;
}

.hero-actions {
  display: flex;
  gap: 16px;
  justify-content: center;
}

.btn-primary, .btn-secondary {
  padding: 16px 32px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 1.1rem;
  text-decoration: none;
  transition: all 0.3s;
  display: inline-block;
}

.btn-primary {
  background: white;
  color: #1a5fb4;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

.btn-secondary {
  background: transparent;
  color: white;
  border: 2px solid white;
}

.btn-secondary:hover {
  background: white;
  color: #1a5fb4;
}

/* Stats */
.stats {
  background: #f8fafc;
  padding: 60px 0;
  margin-top: -60px;
  position: relative;
  z-index: 1;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 30px;
  text-align: center;
}

.stat-number {
  display: block;
  font-size: 2.5rem;
  font-weight: 800;
  color: #1a5fb4;
  margin-bottom: 8px;
}

.stat-label {
  color: #64748b;
  font-size: 0.95rem;
}

/* About */
.about {
  padding: 100px 0;
  background: white;
}

.section-header {
  text-align: center;
  margin-bottom: 60px;
}

.section-header h2 {
  font-size: 2.5rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 12px;
}

.section-header p {
  color: #64748b;
  font-size: 1.1rem;
  max-width: 600px;
  margin: 0 auto;
}

.about-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
}

.about-card {
  padding: 40px 24px;
  border-radius: 16px;
  background: #f8fafc;
  text-align: center;
  transition: all 0.3s;
}

.about-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px rgba(0,0,0,0.1);
}

.about-card .icon {
  font-size: 3rem;
  margin-bottom: 20px;
  display: block;
}

.about-card h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 12px;
}

.about-card p {
  color: #64748b;
  line-height: 1.6;
}

/* Services */
.services {
  padding: 100px 0;
  background: #f8fafc;
}

.services-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}

.service-card {
  background: white;
  border-radius: 16px;
  padding: 40px 30px;
  position: relative;
  box-shadow: 0 4px 6px rgba(0,0,0,0.05);
  transition: all 0.3s;
}

.service-card:hover {
  box-shadow: 0 20px 40px rgba(0,0,0,0.1);
}

.service-card.featured {
  border: 2px solid #1a5fb4;
  transform: scale(1.02);
}

.badge {
  position: absolute;
  top: -12px;
  right: 24px;
  background: #1a5fb4;
  color: white;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
}

.service-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid #e2e8f0;
}

.service-header h3 {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1e293b;
}

.price {
  font-size: 1.125rem;
  font-weight: 700;
  color: #1a5fb4;
}

.service-card ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.service-card li {
  padding: 12px 0;
  border-bottom: 1px solid #f1f5f9;
  color: #475569;
  display: flex;
  align-items: center;
  gap: 8px;
}

.service-card li:before {
  content: "✓";
  color: #22c55e;
  font-weight: 700;
}

/* Process */
.process {
  padding: 100px 0;
  background: white;
}

.process-steps {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
}

.step {
  text-align: center;
  padding: 30px;
  min-width: 180px;
}

.step-number {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 60px;
  height: 60px;
  background: #1a5fb4;
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
  border-radius: 50%;
  margin: 0 auto 16px;
}

.step h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
}

.step p {
  color: #64748b;
  font-size: 0.9rem;
  line-height: 1.5;
}

.step-arrow {
  font-size: 2rem;
  color: #cbd5e1;
}

/* Contact */
.contact {
  padding: 100px 0;
  background: linear-gradient(135deg, #1a5fb4 0%, #1c71d8 100%);
  color: white;
}

.contact .section-header h2,
.contact .section-header p {
  color: white;
}

.contact-wrapper {
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  gap: 60px;
  align-items: start;
}

.contact-form {
  background: white;
  padding: 40px;
  border-radius: 16px;
  color: #1e293b;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 8px;
  color: #374151;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #1a5fb4;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.btn-submit {
  width: 100%;
  padding: 16px;
  background: #1a5fb4;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-submit:hover {
  background: #1c71d8;
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

.btn-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.contact-info {
  padding: 40px 0;
}

.contact-info h3 {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 30px;
}

.info-item {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
}

.info-item .icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.info-item strong {
  display: block;
  margin-bottom: 4px;
  font-weight: 600;
}

.info-item p {
  opacity: 0.9;
  line-height: 1.5;
}

/* Footer */
.footer {
  background: #0f172a;
  color: white;
  padding: 40px 0;
}

.footer-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.25rem;
  font-weight: 700;
}

.copyright {
  opacity: 0.6;
  font-size: 0.9rem;
}

/* Responsive */
@media (max-width: 1024px) {
  .about-grid,
  .services-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .contact-wrapper {
    grid-template-columns: 1fr;
  }
  
  .hero h1 {
    font-size: 2.5rem;
  }
}

@media (max-width: 768px) {
  .navbar .container {
    flex-direction: column;
    gap: 20px;
  }
  
  .nav-links {
    flex-wrap: wrap;
    justify-content: center;
  }
  
  .hero h1 {
    font-size: 2rem;
  }
  
  .hero-actions {
    flex-direction: column;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .about-grid,
  .services-grid {
    grid-template-columns: 1fr;
  }
  
  .process-steps {
    flex-direction: column;
  }
  
  .step-arrow {
    transform: rotate(90deg);
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .footer-content {
    flex-direction: column;
    gap: 20px;
    text-align: center;
  }
}
</style>
