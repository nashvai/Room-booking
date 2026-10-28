import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import NavBar from "./components/NavBar";
import RoomsPage from "./pages/RoomsPage";
import UsersPage from "./pages/UsersPage";
import ReservationsPage from "./pages/ReservationsPage";
import UsagePage from "./pages/UsagePage";
import "./App.css";

export default function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<Navigate to="/rooms" replace />} />
        <Route path="/rooms" element={<RoomsPage />} />
        <Route path="/users" element={<UsersPage />} />
        <Route path="/reservations" element={<ReservationsPage />} />
        <Route path="/usage" element={<UsagePage />} />
        <Route path="*" element={<Navigate to="/rooms" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
