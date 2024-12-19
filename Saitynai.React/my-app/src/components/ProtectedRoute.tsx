import React, { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import api from '../services/api';

interface ProtectedRouteProps {
	children: React.ReactNode;
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
	const [loggedIn, setLoggedIn] = useState(true);

	useEffect(() => {
		async function isLoggedIn(){
			try {
				var response = await api.get("api/is-logged-in");
				setLoggedIn(response.status === 200);
			}
			catch {
				setLoggedIn(false);
			}
		}

		isLoggedIn();
	}, []);


	if (!loggedIn) {
		return <Navigate to="/login" replace />;
	}

	return <>{children}</>;
};

export default ProtectedRoute;
