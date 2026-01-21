import React from 'react';
import './ContactPage.css';

const ContactPage = () => {
  const teamMembers = [
    {
      name: 'Rusu Andrei',
      role: 'Full Stack Developer',
      description: 'Backend architecture, API development, Azure integration',
      icon: 'bi-code-slash'
    },
    {
      name: 'Plesa Valentin',
      role: 'Full Stack Developer',
      description: 'Frontend development, Payment processing, UI/UX design',
      icon: 'bi-palette'
    },
    {
      name: 'Simedre Patricia',
      role: 'Full Stack Developer',
      description: 'Database design, Testing, Documentation',
      icon: 'bi-database'
    }
  ];

  return (
    <div className="contact-page">
      {/* Header */}
      <div className="contact-header text-center mb-5">
        <h1 className="display-4 fw-bold text-white mb-3">
          <i className="bi bi-envelope-fill me-3" style={{color: '#e94560'}}></i>
          Contact Us
        </h1>
        <p className="lead text-muted">
          Get in touch with our team
        </p>
      </div>

      {/* Team Section */}
      <div className="team-section mb-5">
        <h2 className="fw-bold text-white mb-4 text-center">
          <i className="bi bi-people-fill me-2" style={{color: '#e94560'}}></i>
          Our Team
        </h2>
        <p className="text-center text-muted mb-5">
          ShopVRG was created as a project for PSSC (Proiectarea Sistemelor Software Complexe) course
        </p>

        <div className="row row-cols-1 row-cols-md-3 g-4 justify-content-center">
          {teamMembers.map((member, index) => (
            <div key={index} className="col">
              <div className="team-card h-100 text-center p-4" style={{
                background: 'linear-gradient(135deg, #1a1a2e, #16213e)',
                borderRadius: '16px',
                border: '1px solid rgba(233, 69, 96, 0.2)'
              }}>
                <div className="team-avatar mx-auto mb-3" style={{
                  width: '100px',
                  height: '100px',
                  background: 'linear-gradient(135deg, #e94560, #ff6b6b)',
                  borderRadius: '50%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <i className={`bi ${member.icon}`} style={{fontSize: '40px', color: '#fff'}}></i>
                </div>
                <h4 className="text-white mb-1">{member.name}</h4>
                <p className="mb-2" style={{color: '#e94560', fontWeight: '600'}}>{member.role}</p>
                <p className="text-muted small mb-0">{member.description}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Location Section */}
      <div className="location-section mb-5">
        <h2 className="fw-bold text-white mb-4 text-center">
          <i className="bi bi-geo-alt-fill me-2" style={{color: '#e94560'}}></i>
          Our Location
        </h2>

        <div className="row g-4">
          <div className="col-lg-5">
            <div className="location-info p-4" style={{
              background: 'linear-gradient(135deg, #1a1a2e, #16213e)',
              borderRadius: '16px',
              border: '1px solid rgba(233, 69, 96, 0.2)',
              height: '100%'
            }}>
              <h4 className="text-white mb-4">
                <i className="bi bi-building me-2" style={{color: '#e94560'}}></i>
                Office Address
              </h4>

              <div className="info-item d-flex align-items-start gap-3 mb-4">
                <i className="bi bi-mortarboard-fill" style={{color: '#e94560', fontSize: '24px'}}></i>
                <div>
                  <h5 className="text-white mb-1">Universitatea Politehnica Timisoara</h5>
                  <p className="text-muted mb-0">Facultatea de Automatica si Calculatoare</p>
                </div>
              </div>

              <div className="info-item d-flex align-items-start gap-3 mb-4">
                <i className="bi bi-pin-map-fill" style={{color: '#e94560', fontSize: '24px'}}></i>
                <div>
                  <p className="text-white mb-1">Bulevardul Vasile Parvan 2</p>
                  <p className="text-muted mb-0">300223 Timisoara, Romania</p>
                </div>
              </div>

              <div className="info-item d-flex align-items-start gap-3 mb-4">
                <i className="bi bi-telephone-fill" style={{color: '#e94560', fontSize: '24px'}}></i>
                <div>
                  <p className="text-white mb-0">+40 256 403 000</p>
                </div>
              </div>

              <div className="info-item d-flex align-items-start gap-3 mb-4">
                <i className="bi bi-envelope-fill" style={{color: '#e94560', fontSize: '24px'}}></i>
                <div>
                  <p className="text-white mb-0">contact@cs.upt.ro</p>
                </div>
              </div>

              <div className="info-item d-flex align-items-start gap-3">
                <i className="bi bi-globe" style={{color: '#e94560', fontSize: '24px'}}></i>
                <div>
                  <a href="https://ac.upt.ro" target="_blank" rel="noopener noreferrer" className="text-white text-decoration-none">
                    ac.upt.ro
                  </a>
                </div>
              </div>
            </div>
          </div>

          <div className="col-lg-7">
            <div className="map-container" style={{
              borderRadius: '16px',
              overflow: 'hidden',
              border: '1px solid rgba(233, 69, 96, 0.2)',
              height: '100%',
              minHeight: '400px'
            }}>
              <iframe
                title="UPT Location"
                src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2783.8661!2d21.2280!3d45.7472!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x47455d84610655bf%3A0xfd169ff24d29f192!2sFacultatea%20de%20Automatic%C4%83%20%C8%99i%20Calculatoare%2C%20Universitatea%20Politehnica%20Timi%C8%99oara!5e0!3m2!1sro!2sro!4v1700000000000!5m2!1sro!2sro"
                width="100%"
                height="100%"
                style={{border: 0, minHeight: '400px'}}
                allowFullScreen
                loading="lazy"
                referrerPolicy="no-referrer-when-downgrade"
              ></iframe>
            </div>
          </div>
        </div>
      </div>

      {/* Course Info */}
      <div className="course-section text-center p-5" style={{
        background: 'linear-gradient(135deg, #1a1a2e, #16213e)',
        borderRadius: '16px',
        border: '1px solid rgba(233, 69, 96, 0.2)'
      }}>
        <h3 className="text-white mb-3">
          <i className="bi bi-journal-code me-2" style={{color: '#e94560'}}></i>
          About This Project
        </h3>
        <p className="text-muted mb-4">
          ShopVRG is a comprehensive e-commerce platform developed as part of the
          <strong className="text-white"> PSSC (Proiectarea Sistemelor Software Complexe)</strong> course
          at Universitatea Politehnica Timisoara.
        </p>
        <div className="row justify-content-center">
          <div className="col-md-8">
            <div className="d-flex flex-wrap justify-content-center gap-3">
              <span className="badge" style={{background: 'rgba(233, 69, 96, 0.2)', color: '#e94560', padding: '8px 16px'}}>
                <i className="bi bi-filetype-cs me-1"></i> .NET 9
              </span>
              <span className="badge" style={{background: 'rgba(233, 69, 96, 0.2)', color: '#e94560', padding: '8px 16px'}}>
                <i className="bi bi-filetype-tsx me-1"></i> React TypeScript
              </span>
              <span className="badge" style={{background: 'rgba(233, 69, 96, 0.2)', color: '#e94560', padding: '8px 16px'}}>
                <i className="bi bi-cloud me-1"></i> Azure SQL
              </span>
              <span className="badge" style={{background: 'rgba(233, 69, 96, 0.2)', color: '#e94560', padding: '8px 16px'}}>
                <i className="bi bi-lightning me-1"></i> Azure Service Bus
              </span>
              <span className="badge" style={{background: 'rgba(233, 69, 96, 0.2)', color: '#e94560', padding: '8px 16px'}}>
                <i className="bi bi-diagram-3 me-1"></i> DDD Architecture
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ContactPage;
