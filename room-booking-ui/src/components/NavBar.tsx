import { NavLink } from "react-router-dom";

const links = [
  ["/rooms", "Rooms"],
  ["/users", "Users"],
  ["/reservations", "Reservations"],
  ["/usage", "Room usage"],
] as const;

export default function NavBar() {
  return (
    <nav className="top-nav" aria-label="Main navigation">
      <NavLink to="/rooms" className="brand">Room<span>Book</span></NavLink>
      <div className="nav-links">
        {links.map(([to, label]) => <NavLink key={to} to={to} className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>{label}</NavLink>)}
      </div>
    </nav>
  );
}
