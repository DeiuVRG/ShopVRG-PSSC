import React from 'react';
import './ContactPage.css';

const ContactPage = () => {
  const teamMembers = [
    { name: 'Rusu Andrei' },
    { name: 'Plesa Valentin' },
    { name: 'Simedre Patricia' }
  ];

  return (
    <div className="contact-page">
      {/* Header */}
      <div className="contact-header">
        <h1>
          <i className="bi bi-envelope-fill"></i>
          Contact
        </h1>
        <p>Echipa noastra este aici pentru tine!</p>
      </div>

      {/* Team Section */}
      <div className="team-section">
        <h2>
          <i className="bi bi-people-fill"></i>
          Echipa de Dezvoltare
        </h2>
        <p className="team-subtitle">ShopVRG - Proiect PSSC</p>

        <div className="team-grid">
          {teamMembers.map((member, index) => (
            <div key={index} className="team-card">
              <div className="team-avatar">
                <i className="bi bi-person-fill"></i>
              </div>
              <h4>{member.name}</h4>
            </div>
          ))}
        </div>
      </div>

      {/* Location Section */}
      <div className="location-section">
        <h2>
          <i className="bi bi-geo-alt-fill"></i>
          Locatia Noastra
        </h2>

        <div className="location-grid">
          <div className="address-card">
            <h3>
              <i className="bi bi-building"></i>
              Informatii Contact
            </h3>

            <div className="address-list">
              <div className="address-item">
                <i className="bi bi-mortarboard-fill"></i>
                <div>
                  <strong>Universitatea Politehnica Timisoara</strong>
                  <span>Facultatea de Automatica si Calculatoare</span>
                </div>
              </div>

              <div className="address-item">
                <i className="bi bi-geo-alt-fill"></i>
                <div>
                  <strong>Bulevardul Vasile Parvan Nr. 2</strong>
                  <span>300223 Timisoara, Romania</span>
                </div>
              </div>

              <div className="address-item">
                <i className="bi bi-telephone-fill"></i>
                <div>
                  <strong>+40 256 403 000</strong>
                </div>
              </div>

              <div className="address-item">
                <i className="bi bi-envelope-fill"></i>
                <div>
                  <strong>contact@shopvrg.ro</strong>
                </div>
              </div>

              <div className="address-item">
                <i className="bi bi-clock-fill"></i>
                <div>
                  <strong>Program</strong>
                  <span>Luni - Vineri: 08:00 - 18:00</span>
                </div>
              </div>
            </div>
          </div>

          <div className="map-card">
            <iframe
              title="UPT AC Location"
              src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2784.0876543210987!2d21.22650001555896!3d45.74650007910544!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x47455d84610655bf%3A0x4c8a2e59e2fa5a6c!2sFacultatea%20de%20Automatic%C4%83%20%C8%99i%20Calculatoare!5e0!3m2!1sro!2sro!4v1642696547012!5m2!1sro!2sro"
              width="100%"
              height="100%"
              style={{border: 0}}
              allowFullScreen
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
            ></iframe>
          </div>
        </div>
      </div>

      {/* Course Info */}
      <div className="course-section">
        <h3>
          <i className="bi bi-journal-code"></i>
          Despre Proiect
        </h3>
        <p>
          ShopVRG este o platforma e-commerce dezvoltata pentru cursul
          <strong> PSSC (Proiectarea Sistemelor Software Complexe)</strong> la
          Universitatea Politehnica Timisoara.
        </p>
        <div className="tech-badges">
          <span className="badge"><i className="bi bi-filetype-cs"></i> .NET 9</span>
          <span className="badge"><i className="bi bi-filetype-tsx"></i> React TypeScript</span>
          <span className="badge"><i className="bi bi-cloud"></i> Azure SQL</span>
          <span className="badge"><i className="bi bi-lightning"></i> Azure Service Bus</span>
          <span className="badge"><i className="bi bi-diagram-3"></i> DDD Architecture</span>
        </div>
      </div>
    </div>
  );
};

export default ContactPage;
