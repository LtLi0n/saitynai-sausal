import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import TagsList from './pages/TagsList';
import ProtectedRoute from './components/ProtectedRoute';
import NotesList from './pages/NotesList';
import Note from './pages/Note';
import NavigationBar from './components/NavigationBar';

const App: React.FC = () => {
  return (
    <Router>
      <NavigationBar />
      <Routes>
        <Route path="/login" element={<Login />} />

        <Route path="/dashboard" element={<ProtectedRoute> <Dashboard /> </ProtectedRoute>} />
        <Route path="/notes" element={<ProtectedRoute> <NotesList /> </ProtectedRoute>} />
        <Route path="/notes/create" element={<ProtectedRoute> <Note /> </ProtectedRoute>} />
        <Route path="/notes/:noteId" element={<ProtectedRoute> <Note /> </ProtectedRoute>} />
        <Route path="/tags" element={<ProtectedRoute> <TagsList /> </ProtectedRoute>} />

      </Routes>
    </Router>
  );
};

export default App;
