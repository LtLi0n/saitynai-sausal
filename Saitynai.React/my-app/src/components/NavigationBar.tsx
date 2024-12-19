import React, { useState } from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';

interface IItem {
	name: string;
	uri: string;
}

const NavigationBar: React.FC = () => {
	const navigate = useNavigate();

	const [isAdmin, setIsAdmin] = useState(false);
	const [loadedToken, setLoadedToken] = useState<string | null>(null);

	const token = localStorage.getItem('token')!;
	if (token !== loadedToken) {
		if (token) {
			const userInfo = jwtDecode(token);
			console.log(userInfo);
			setIsAdmin(userInfo.is_admin === "true");
			setLoadedToken(token);
		}
	}

	if (!loadedToken) {
		return (<></>)
	}

	const handleLogout = () => {
		localStorage.removeItem('token');
		setLoadedToken(null);
		navigate('/login');
	};

	return (
		<AppBar position="fixed" color="primary">
			<Toolbar>
				<Typography
					variant="h6"
					onClick={() => navigate('/notes')}
					sx={{ flexGrow: 1, cursor: 'pointer', fontWeight: 'bold' }}>
					Notes App
				</Typography>

				<Box>
					<Button
						color="inherit"
						onClick={() => navigate('/dashboard')}
						sx={{ '&:hover': { backgroundColor: 'rgba(255, 255, 255, 0.2)' } }}
					>
						Home
					</Button>
					<Button
						color="inherit"
						onClick={() => navigate('/notes')}
						sx={{ '&:hover': { backgroundColor: 'rgba(255, 255, 255, 0.2)' } }}
					>
						Notes
					</Button>
					<Button
						color="inherit"
						onClick={() => navigate('/tags')}
						sx={{ '&:hover': { backgroundColor: 'rgba(255, 255, 255, 0.2)' } }}
					>
						Tags
					</Button>
					<Button color="inherit" onClick={handleLogout}>
						Logout
					</Button>
				</Box>
			</Toolbar>
		</AppBar>
	);
};

export default NavigationBar;
