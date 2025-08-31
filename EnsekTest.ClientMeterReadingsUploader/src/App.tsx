import React from 'react';
import './App.css';
import FileUploadForm from './components/FileUploadForm';

const App: React.FC = () => {
  // Read endpoint from Vite environment variable
  const endpoint = import.meta.env.VITE_UPLOAD_ENDPOINT || '';

  return (
    <div className="upload-container">
      <h3>Upload meter readings</h3>
      <FileUploadForm endpoint={endpoint} />
      {endpoint ? (
        <p style={{ marginTop: '20px', fontSize: '0.9em', color: '#888' }}>
          Upload endpoint: <code>{endpoint}</code>
        </p>
      ) : (
        <p style={{ marginTop: '20px', color: 'red' }}>
          No upload endpoint set. Please set <b>VITE_UPLOAD_ENDPOINT</b> in your .env file.
        </p>
      )}
    </div>
  );
};

export default App;
