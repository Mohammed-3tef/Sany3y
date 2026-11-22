import React from "react";

const TEAM_MEMBERS = [
  {
    image: "/img/team_members/kenzi.jpg",
    name: "Kenzi Shibl",
    role: "Front-End Developer & Content Designer",
    description:
      "Kenzi is the creative force behind Sany3y, managing every detail with passion and precision. Her vision and leadership shape the team’s direction, ensuring every project reflects innovation and authenticity.",
    email: "kenzi@sany3y.com",
    linkedin: "https://www.linkedin.com/in/kenzi-shibl/",
  },
  {
    image: "/img/team_members/omnia.jpg",
    name: "Omnia Ashraf",
    role: "UI/UX Designer",
    description:
      "Omnia brings designs to life with her eye for simplicity and elegance. She crafts digital experiences that feel natural and beautiful, ensuring every user feels connected to the brand’s story.",
    email: "omnia@sany3y.com",
    linkedin: "https://www.linkedin.com/in/omniaashraf2/",
  },
  {
    image: "/img/team_members/mohammed.jpg",
    name: "Mohammed Atef",
    role: ".Net Back-End Developer",
    description:
      "Mohamed turns imagination into code. With a deep understanding of structure and interaction, he makes the impossible look easy. His clean, efficient code ensures the website runs smoothly.",
    email: "mohamed45452020@gmail.com",
    linkedin: "https://www.linkedin.com/in/mohammed-atef-abd-elkader/",
  },
  {
    image: "/img/team_members/steven.jpg",
    name: "Steven Amin",
    role: ".Net Back-End Developer",
    description:
      "Steven is the logic master of Sany3y. He builds the strong technical foundation that powers every feature. With his problem-solving mindset, he ensures stability, security, and performance.",
    email: "stevenameen02@gmail.com",
    linkedin: "https://www.linkedin.com/in/steven-amin02/",
  },
];

export default function TeamPage() {
  return (
    <div className="container pt-5 pb-5 mt-5">
      <h1 className="my-4 fw-bold" style={{ color: "var(--primary-green)" }}>
        فريق العمل
      </h1>

      <div className="row gy-4 justify-content-center">
        {TEAM_MEMBERS.map((member) => (
          <div key={member.name} className="col-md-6 col-lg-3 d-flex text-start">
            <div className="card border bg-white flex-fill d-flex flex-column shadow-sm">
              <div className="card-body d-flex flex-column" dir="ltr">
                <img
                  src={member.image}
                  alt={member.name}
                  className="rounded-3 mb-3"
                  style={{ width: "100%", height: 280, objectFit: "cover" }}
                />
                <h3 className="text-success fw-bold fs-4">{member.name}</h3>
                <h6 className="text-muted mb-3">{member.role}</h6>
                <p className="flex-grow-1 text-secondary small">
                  {member.description}
                </p>
                <div className="d-flex gap-3 mt-2">
                  <a
                    href={`mailto:${member.email}`}
                    className="text-dark fs-5"
                    target="_blank"
                    rel="noreferrer"
                  >
                    <i className="fa-solid fa-envelope"></i>
                  </a>
                  <a
                    href={member.linkedin}
                    className="text-primary fs-5"
                    target="_blank"
                    rel="noreferrer"
                  >
                    <i className="fa-brands fa-linkedin"></i>
                  </a>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
