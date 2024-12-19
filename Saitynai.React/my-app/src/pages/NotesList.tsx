import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, Typography, CircularProgress, Grid2, Button, Grid, Box, TextField } from '@mui/material';
import api from '../services/api';

interface Note {
	id: string;
	content: string;
	tags: string[];
}

const NotesList: React.FC = () => {
	const [notes, setNotes] = useState<Note[]>([]);
	const [loading, setLoading] = useState<boolean>(true);
	const [searchQuery, setSearchQuery] = useState<string>('');
	const [error, setError] = useState<string | null>(null);
	const navigate = useNavigate();

	const fetchNotes = async () => {
		setLoading(true);
		setError(null);
		try {
			const response = await api.get('/notes', {
				params: { Search: searchQuery },
			});
			setNotes(response.data);
		} catch (error) {
			console.error('Error fetching notes:', error);
			setError('Failed to fetch notes. Please try again.');
		} finally {
			setLoading(false);
		}
	};

	const handleSearch = () => {
		fetchNotes();
	};

	return (
		<div style={{ padding: '2rem', marginTop: 50 }}>
			{/* Search Bar */}
			<Box display="flex" marginBottom="1rem">
				<TextField
					fullWidth
					placeholder="Search notes..."
					variant="outlined"
					value={searchQuery}
					onChange={(e) => setSearchQuery(e.target.value)}
					sx={{ marginRight: '1rem' }}
				/>
				<Button variant="contained" color="primary" onClick={handleSearch}>
					Search
				</Button>
			</Box>
			<Typography variant="h4" component="h1" gutterBottom>
				My Notes
			</Typography>

			<Button
				variant="contained"
				color="primary"
				style={{ marginBottom: '1rem' }}
				onClick={() => navigate('/notes/create')}
			>Create New Note</Button>
			<Grid2 container spacing={3}>
				{notes.map((note) => (
					<Grid item xs={12} sm={6} md={4} key={note.id}>
						<Card
							sx={{
								transition: 'transform 0.3s, box-shadow 0.3s, background-color 0.3s',
								'&:hover': {
									transform: 'scale(1.05)',
									boxShadow: 6,
									backgroundColor: 'rgba(67, 89, 213, 0.7)', // Light blue background
								},
							}}
							onClick={() => navigate(`/notes/${note.id}`)}
						>
							<CardContent>
								<Typography variant="h6" gutterBottom>
									Note ID: {note.id}
								</Typography>
								<Typography variant="body2" color="textSecondary">
									{note.content.substring(0, 100)}{note.content.length > 100 ? "..." : ""}
								</Typography>
								<Typography variant="caption" color="primary">
									{note.tags.join(", ")}
								</Typography>
							</CardContent>
						</Card>
					</Grid>
				))}
			</Grid2>
		</div>
	);
};

export default NotesList;
